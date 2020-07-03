using MediatR;
using RSPeer.Common.Enums;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Store.Purchase.Commands
{
    public class ModifyPricePerQuantityCommand : IRequest<Unit>
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public User User { get; set; }
        
        public CrudAction Action { get; set; }
    }
}