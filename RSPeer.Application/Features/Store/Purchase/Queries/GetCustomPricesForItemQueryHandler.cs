using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Purchase.Queries
{
    public class GetCustomPricesForItemQueryHandler : IRequestHandler<GetCustomPricesForItemQuery, IEnumerable<PricePerQuantity>>
    {
        private readonly RsPeerContext _db;

        public GetCustomPricesForItemQueryHandler(RsPeerContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<PricePerQuantity>> Handle(GetCustomPricesForItemQuery request, CancellationToken cancellationToken)
        {
            return await _db.PricePerQuantity.Where(w => w.Sku == request.Sku)
                .OrderBy(w => w.Quantity)
                .ToListAsync(cancellationToken);
        }
    }
}