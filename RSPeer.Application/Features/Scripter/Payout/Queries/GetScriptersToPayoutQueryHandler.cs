using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripter.Payout.Queries
{
	public class GetScriptersToPayoutQueryHandler : IRequestHandler<GetScriptersToPayoutQuery, IEnumerable<ScripterPayout>>
	{
		private readonly RsPeerContext _db;

		public GetScriptersToPayoutQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<ScripterPayout>> Handle(GetScriptersToPayoutQuery request,
			CancellationToken cancellationToken)
		{
			var scripts = (await _db.Scripts.Where(w => w.Type == ScriptType.Premium).ToListAsync(cancellationToken))
				.ToDictionary(w => "premium-script-" + w.Id, w => w);

			var scripterUserIds = scripts.Values.Select(w => w.UserId).ToList();

			var scripters = await _db.Users.Where(w => scripterUserIds.Contains(w.Id)).ToListAsync(cancellationToken);

			var skus = scripts.Keys.ToList();

			var items = await _db.Items.Where(w => skus.Contains(w.Sku)).ToListAsync(cancellationToken);

			var orderItemIds = items.Select(w => w.Id);

			var orders = await _db.Orders.Where(w => w.Status == OrderStatus.Completed && w.Quantity > 0 
			                                                                           && !w.IsPaidOut && orderItemIds.Contains(w.ItemId))
				.Include(w => w.User).ThenInclude(w => w.UserGroups).ThenInclude(w => w.Group)
				.ToListAsync(cancellationToken);

			var result = new List<ScripterPayout>();

			foreach (var scripter in scripters)
			{
				var payout = new ScripterPayout();
				payout.Scripter = scripter;
				var scripterScripts = scripts.Where(w => w.Value.UserId == scripter.Id);
				var scripterItemSkus = scripterScripts.Select(w => w.Key);
				payout.Scripts = scripterScripts.Select(w => w.Value);
				var scripterItemIds = items.Where(w => scripterItemSkus.Contains(w.Sku)).Select(w => w.Id);
				payout.Orders = orders.Where(w => scripterItemIds.Contains(w.ItemId));
				payout.Items = items.Where(w => scripterItemIds.Contains(w.Id));
				result.Add(payout);
			}

			return await ExecuteCalculations(result);
		}

		private async Task<IEnumerable<ScripterPayout>> ExecuteCalculations(IEnumerable<ScripterPayout> payouts)
		{
			var token = 0.01;
			return payouts.Select(p =>
			{
				var total = p.Orders.Where(w => !w.IsRefunded && !w.IsPaidOut && !w.User.IsOwner).Sum(w => w.Total * (decimal) token);
				p.TotalSales = total;
				var balanceAsUsd = new decimal(p.Scripter.Balance * token);
				p.AmountToPayout = balanceAsUsd < total ? balanceAsUsd : total;
				p.TokensToRemove = (int) (p.AmountToPayout / (decimal) token);
				var refunded = p.Orders.Where(w => w.IsRefunded).ToList();
				p.RefundedOrderCount = refunded.Count;
				p.RefundedOrderTotal = refunded.Sum(w => w.Total * (decimal) token);
				var staffPurchases = p.Orders.Where(w => w.User.IsOwner).ToList();
				p.StaffPurchases = staffPurchases.Count;
				p.StaffPurchasesTotal = staffPurchases.Sum(w => w.Total * (decimal) token);
				return p;
			});
		}
	}
}