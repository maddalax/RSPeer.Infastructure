using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Paypal.Queries
{
	public class GetUnfinishedPaypalOrdersQueryHandler : IRequestHandler<GetUnfinishedPaypalOrdersQuery, IEnumerable<Order>>
	{
		private readonly RsPeerContext _db;

		public GetUnfinishedPaypalOrdersQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<Order>> Handle(GetUnfinishedPaypalOrdersQuery request, CancellationToken cancellationToken)
		{
			return await _db.Orders
				.Where(w => w.Status == OrderStatus.Processing).Include(w => w.Item).ToListAsync(cancellationToken);
		}
	}
}