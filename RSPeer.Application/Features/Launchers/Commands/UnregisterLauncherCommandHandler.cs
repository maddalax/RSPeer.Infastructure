using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Launchers.Commands
{
    public class UnregisterLauncherCommandHandler : IRequestHandler<UnregisterLauncherCommand, Unit>
    {
        private readonly IRedisService _redis;

        public UnregisterLauncherCommandHandler(IRedisService redis)
        {
            _redis = redis;
        }

        public async Task<Unit> Handle(UnregisterLauncherCommand request, CancellationToken cancellationToken)
        {
            var key = $"{request.UserId}_{request.Tag}_launcher";
            var connected = $"{request.UserId}_connected_launchers";
            var transaction = _redis.GetDatabase().CreateTransaction();
            transaction.KeyDeleteAsync(key);
            transaction.SetRemoveAsync(connected, request.Tag.ToString());
            await transaction.ExecuteAsync(CommandFlags.FireAndForget);
            return Unit.Value;
        }
    }
}