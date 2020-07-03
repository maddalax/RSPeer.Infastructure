using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Process.Queries
{
	public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IEnumerable<Order>>
	{
		private readonly RsPeerContext _db;

		public GetOrdersQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<Order>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
		{
			var queryable = _db.Orders.AsQueryable();
			if (request.UserId.HasValue)
			{
				queryable = queryable.Where(w => w.UserId == request.UserId.Value);
			}

			if (request.Status.HasValue)
			{
				queryable = queryable.Where(w => w.Status == request.Status.Value);
			}
			
			if (request.ItemId.HasValue)
			{
				queryable = queryable.Where(w => w.ItemId == request.ItemId.Value);
			}

			if (request.NotExpired.HasValue && request.NotExpired.Value)
			{
				request.IncludeItem = true;
			}
			
			if (!string.IsNullOrEmpty(request.Sku))
			{
				request.IncludeItem = true;
				queryable = queryable.Where(w => w.Item.Sku == request.Sku);
			}

			if (request.IncludeItem)
			{
				queryable = queryable.Include(w => w.Item);
			}
			
			var results = await queryable
				.OrderByDescending(w => w.Id)
				.ToListAsync(cancellationToken);

			if (request.NotExpired.HasValue && request.NotExpired.Value)
			{
				results = results.Where(w =>
				{
					if (!w.Item.ExpirationInMinutes.HasValue)
					{
						return true;
					}
					var minutes = w.Item.ExpirationInMinutes.Value;
					return DateTimeOffset.UtcNow < w.Timestamp.AddMinutes(minutes);
				}).ToList();
			}

			return results;
		}
	}
}