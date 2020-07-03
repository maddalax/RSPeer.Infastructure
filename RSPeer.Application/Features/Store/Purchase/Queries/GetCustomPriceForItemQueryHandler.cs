using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Purchase.Queries
{
    public class GetCustomPriceForItemQueryHandler : IRequestHandler<GetCustomPriceForItemQuery, int>
    {
        private readonly RsPeerContext _db;

        public GetCustomPriceForItemQueryHandler(RsPeerContext db)
        {
            _db = db;
        }

        public async Task<int> Handle(GetCustomPriceForItemQuery request, CancellationToken cancellationToken)
        {
            var result = await _db.PricePerQuantity
                .Where(w => w.Sku == request.Item.Sku)
                .Where(w => w.Quantity <= request.Quantity)
                .OrderByDescending(w => w.Quantity).FirstOrDefaultAsync(cancellationToken);
            return (int) (result?.Price ?? request.Item.Price);
        }
    }
}