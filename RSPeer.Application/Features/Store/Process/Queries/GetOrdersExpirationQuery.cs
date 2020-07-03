using System;
using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Store.Process.Queries
{
    public class GetOrdersExpirationQuery : IRequest<DateTimeOffset>
    {
        public string Sku { get; set; }
        public int UserId { get; set; }
        
        public IEnumerable<Order> Orders { get; set; }
    }
}