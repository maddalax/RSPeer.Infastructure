using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.SiteConfig.Queries;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Store.Process.Queries
{
    public class GetOrdersExpirationQueryHandler : IRequestHandler<GetOrdersExpirationQuery, DateTimeOffset>
    {
        private readonly IMediator _mediator;
        private readonly RsPeerContext _db;

        public GetOrdersExpirationQueryHandler(IMediator mediator, RsPeerContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        public async Task<DateTimeOffset> Handle(GetOrdersExpirationQuery request, CancellationToken cancellationToken)
        {
            var query = new GetOrdersQuery
            {
                NotExpired = true, Sku = request.Sku, IncludeItem = true, Status = OrderStatus.Completed,
                UserId = request.UserId
            };
           
            var orders = request.Orders ?? await _mediator.Send(query, cancellationToken);
            
            var preReleaseExpirationRaw = await _mediator.Send(new GetSiteConfigOrThrowCommand
                {Key = "inuvation:pre:expiration:minutes"}, cancellationToken);
            
            var preReleaseExpirationMinutes = long.Parse(preReleaseExpirationRaw);
            
            var preRelease = new HashSet<int>();

            foreach (var order in orders)
            {
                if (order.Item.Sku != "rs3-inuvation-access")
                {
                    continue;
                }
                var key = $"inuvation:prerelease:{order.Id}";
                if (await _db.Data.AnyAsync(w => w.Key == key, cancellationToken))
                {
                    preRelease.Add(order.Id);
                }
            }

            var minutes = orders.Sum(w => preRelease.Contains(w.Id) 
                ? preReleaseExpirationMinutes * w.Quantity 
                : w.Item.ExpirationInMinutes.GetValueOrDefault(int.MaxValue) * w.Quantity);
            
            return minutes == 0 ? DateTimeOffset.MinValue : DateTimeOffset.UtcNow.AddMinutes(minutes);
        }
    }
}