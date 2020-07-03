using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Store.Purchase.Queries
{
    public class GetCustomPriceForItemQuery : IRequest<int>
    {
        public Item Item { get; set; }
        public int Quantity { get; set; }
    }
}