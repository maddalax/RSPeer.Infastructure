using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.UserManagement.Users.Commands;
using RSPeer.Common.Enums;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Scripter.Payout.Commands
{
	public class CompleteScripterPayoutCommandHandler : IRequestHandler<CompleteScripterPayoutCommand, Unit>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public CompleteScripterPayoutCommandHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<Unit> Handle(CompleteScripterPayoutCommand request, CancellationToken cancellationToken)
		{
			var orderIds = request.Payout.Orders.Select(w => w.Id).ToList();
			var orders = await _db.Orders.Where(w => orderIds.Contains(w.Id)).ToListAsync(cancellationToken);
			
			foreach (var order in orders)
			{
				order.IsPaidOut = true;
				order.PayoutDate = DateTimeOffset.UtcNow;
				_db.Orders.Update(order);
			}

			await _mediator.Send(new UserUpdateBalanceCommand
			{
				AdminUserId = request.Admin.Id,
				Amount = request.Payout.TokensToRemove,
				Reason = "scripter:payout",
				Type = AddRemove.Remove,
				UserId = request.Payout.Scripter.Id
			}, cancellationToken);
	
			await _db.SaveChangesAsync(cancellationToken);
			return Unit.Value;
		}
	}
}