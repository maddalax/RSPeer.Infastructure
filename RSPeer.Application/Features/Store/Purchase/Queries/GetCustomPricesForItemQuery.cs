using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Store.Purchase.Queries
{
    public class GetCustomPricesForItemQuery : IRequest<IEnumerable<PricePerQuantity>>
    {
        public string Sku { get; set; }
    }
}