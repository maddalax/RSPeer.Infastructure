using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Process.Queries
{
	public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Order>
	{
		private readonly RsPeerContext _db;

		public GetOrderByIdQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<Order> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
		{
			if (!request.IsAdmin && !request.UserId.HasValue)
			{
				return null;
			}
			
			var queryable = _db.Orders.AsQueryable();
			
			if (request.IncludeItem)
			{
				queryable = queryable.Include(w => w.Item);
			}

			if (!request.IsAdmin)
			{
				queryable = queryable.Where(w => w.UserId == request.UserId);
			}
			
			return await queryable.FirstOrDefaultAsync(w => w.Id == request.OrderId, cancellationToken);
		}
	}
}