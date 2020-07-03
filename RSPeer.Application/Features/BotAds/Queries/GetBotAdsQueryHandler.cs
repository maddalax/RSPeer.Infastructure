using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.BotAds.Models;
using RSPeer.Application.Features.Instance.Queries;
using RSPeer.Application.Features.SiteConfig.Queries;
using RSPeer.Application.Features.UserData.Queries;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Common.Extensions;

namespace RSPeer.Application.Features.BotAds.Queries
{
    public class GetBotAdsQueryHandler : IRequestHandler<GetBotAdsQuery, BotAdResult>
    {
        private readonly IMediator _mediator;

        public GetBotAdsQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<BotAdResult> Handle(GetBotAdsQuery request, CancellationToken cancellationToken)
        {
            var user = await _mediator.Send(new GetUserByIdQuery
            {
                Id = request.UserId
            }, cancellationToken);

            if (!user.IsOwner)
            {
                var enabled = await _mediator.Send(new GetSiteConfigOrThrowCommand
                {
                    Key = "bot_ads:enabled"
                }, cancellationToken);

                if (enabled != null && bool.TryParse(enabled, out var b) && !b)
                {
                    return new BotAdResult();
                }
            }

            var links = await _mediator.Send(new GetUserJsonDataQuery
            {
                Key = "internal_bot_ads",
                UserId = 1189
            }, cancellationToken);
            
            var ads = new BotAdResult
            {
                Ads = links.To<IEnumerable<BotAd>>().Where(w => !w.Expired && w.Enabled)
            };

            var instances = await _mediator.Send(new GetAllowedInstancesQuery { IncludeFree = false, UserId = request.UserId }, cancellationToken);
            var running = await _mediator.Send(new GetRunningClientsQuery{ UserId = request.UserId }, cancellationToken);
            var list = running.ToList().OrderByDescending(w => w.Tag).ToList();
            
            if (!ads.Ads.Any() || list.Count <= instances)
            {
                ads.ShouldShow = false;
                return ads;
            }

            var free = list.Skip(instances);
            ads.ShouldShow = free.Any(w => w.Tag == request.ClientTag);
            
            return ads;
        }
    }
}