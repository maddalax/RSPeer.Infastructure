using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Items.Queries
{
	public class GetItemBySkuQueryHandler : IRequestHandler<GetItemBySkuQuery, Item>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;

		public GetItemBySkuQueryHandler(RsPeerContext db, IRedisService redis)
		{
			_db = db;
			_redis = redis;
		}

		public async Task<Item> Handle(GetItemBySkuQuery request, CancellationToken cancellationToken)
		{
			var item = new Func<Task<Item>>(() => _db.Items.FirstOrDefaultAsync(w => w.Sku == request.Sku, cancellationToken));
			return request.AllowCached 
				? await _redis.GetOrDefault($"items_sku_{request.Sku}", item, TimeSpan.FromHours(24)) 
				: await item.Invoke();
		}
	}
}