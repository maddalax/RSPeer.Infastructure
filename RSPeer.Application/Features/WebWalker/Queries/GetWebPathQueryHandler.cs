using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RSPeer.Application.Features.UserData.Queries;
using RSPeer.Application.Features.WebWalker.Acuity;
using RSPeer.Application.Features.WebWalker.Base;
using RSPeer.Application.Features.WebWalker.DaxWalker;
using RSPeer.Application.Infrastructure.Caching.Base;

namespace RSPeer.Application.Features.WebWalker.Queries
{
    public class GetWebPathQueryHandler : IRequestHandler<GetWebPathQuery, GetWebPathResult>
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IMediator _mediator;
        private readonly IRedisService _redis;

        public GetWebPathQueryHandler(IServiceScopeFactory factory, IMediator mediator, IRedisService redis)
        {
            _factory = factory;
            _mediator = mediator;
            _redis = redis;
        }

        public async Task<GetWebPathResult> Handle(GetWebPathQuery request, CancellationToken cancellationToken)
        {
            var prefs = await _mediator.Send(new GetBotPreferencesQuery
            {
                UserId = request.UserId
            }, cancellationToken);

            var webWalkerType = request.WebWalker.HasValue && request.WebWalker != Domain.Entities.WebWalker.ClientSettingsBased
                ? request.WebWalker.Value : prefs.WebWalker;

            var walker = GetWalker(webWalkerType);
            var result = request.Type == WebPathType.Normal
                ? await walker.GeneratePath(request.Payload, prefs.DaxWebKey)
                : await walker.GenerateBankPath(request.Payload, prefs.DaxWebKey);
            await _redis.Increment($"path_requests_{webWalkerType.ToString()}");
            return new GetWebPathResult
            {
                Result = result,
                Walker = webWalkerType
            };
        }

        private IWebWalker GetWalker(Domain.Entities.WebWalker walker)
        {
            using var scope = _factory.CreateScope();
            return walker switch
            {
                Domain.Entities.WebWalker.Dax => (IWebWalker) scope.ServiceProvider.GetService<DaxWalkerService>(),
                Domain.Entities.WebWalker.Acuity => scope.ServiceProvider.GetService<AcuityWalkerService>(),
                _ => throw new ArgumentOutOfRangeException(nameof(walker), walker, null)
            };
        }
    }
}