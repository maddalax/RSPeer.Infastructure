using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Infrastructure.Caching.Base;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
    public class GetUserIdBySessionQueryHandler : IRequestHandler<GetUserIdBySessionQuery, int?>
    {
        private readonly IRedisService _redis;
        private readonly IMediator _mediator;

        public GetUserIdBySessionQueryHandler(IRedisService redis, IMediator mediator)
        {
            _redis = redis;
            _mediator = mediator;
        }

        public async Task<int?> Handle(GetUserIdBySessionQuery request, CancellationToken cancellationToken)
        {
            var key = $"session_userId_{request.Session}";
            if (request.AllowCached)
            {
                var exists = await _redis.GetString(key);
                if (exists != null && int.TryParse(exists, out var userId))
                {
                    return userId;
                }
            }

            var user = await _mediator.Send(new GetUserBySessionOrApiKeyQuery
            {
                Session = request.Session
            }, cancellationToken);

            if (user == null)
            {
                return null;
            }
            
            await _redis.Set(key, user.Id);
            return user.Id;
        }
    }
}