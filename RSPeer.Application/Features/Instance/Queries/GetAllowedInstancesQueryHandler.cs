using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Store.Items.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Instance.Queries
{
	public class GetAllowedInstancesQueryHandler : IRequestHandler<GetAllowedInstancesQuery, int>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _cache;
		private readonly IMediator _mediator;

		public GetAllowedInstancesQueryHandler(RsPeerContext db, IRedisService cache, IMediator mediator)
		{
			_db = db;
			_cache = cache;
			_mediator = mediator;
		}

		public async Task<int> Handle(GetAllowedInstancesQuery request, CancellationToken cancellationToken)
		{
			var key = request.IncludeFree
				? $"{request.UserId}_instances_allowed"
				: $"{request.UserId}_instances_allowed_no_free";

			var value = await _cache.GetOrDefault(key, async () =>
			{
				var instances = await _mediator.Send(new GetItemBySkuQuery { Sku = "instances", AllowCached = true },
					cancellationToken);
				var timestamp = DateTimeOffset.UtcNow.AddMinutes((double) -instances.ExpirationInMinutes);
				var count = await _db.Orders.Where(w =>
						w.ItemId == instances.Id 
						&& w.Status == OrderStatus.Completed
						&& !w.IsRefunded
						&& w.UserId == request.UserId && w.Timestamp >= timestamp)
					.SumAsync(w => w.Quantity, cancellationToken);
				if (request.IncludeFree)
				{
					count += + await CalculateFreeInstances(request.UserId);
				}

				return count;
			}, TimeSpan.FromHours(1));

			if (request.IncludeFree && value <= 0)
			{
				await _cache.Remove($"{request.UserId}_instances_allowed");
				return 1000000;
			}

			return value;
		}

		private async Task<int> CalculateFreeInstances(int userId)
		{
			if (await _db.DiscordAccounts.FirstOrDefaultAsync(w => w.UserId == userId) != null)
			{
				var verified = await _db.SiteConfig.Where(w => w.Key == "discordVerifiedInstances").Select(w => w.Value).FirstOrDefaultAsync();
				return int.Parse(verified);
			}
			var free = await _db.SiteConfig.Where(w => w.Key == "freeInstances").Select(w => w.Value).FirstOrDefaultAsync();
			return int.Parse(free);
		}
	}
}