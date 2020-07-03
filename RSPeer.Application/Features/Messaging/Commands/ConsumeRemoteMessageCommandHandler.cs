using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Messaging.Commands
{
    public class ConsumeRemoteMessageCommandHandler : IRequestHandler<ConsumeRemoteMessageCommand, Unit>
    {
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;

        public ConsumeRemoteMessageCommandHandler(RsPeerContext db, IRedisService redis)
        {
            _db = db;
            _redis = redis;
        }

        public async Task<Unit> Handle(ConsumeRemoteMessageCommand request, CancellationToken cancellationToken)
        {
            var key = $"messages_{request.UserId}";
            var messageIds = await _redis.GetSet(key);
            var filtered = messageIds.FirstOrDefault(w => w == request.MessageId.ToString());

            if (filtered == null)
            {
                return Unit.Value;
            }
            
            var transaction = _redis.GetDatabase().CreateTransaction();
            transaction.SetRemoveAsync(key, filtered);
            transaction.KeyDeleteAsync($"messages_{request.UserId}_{filtered}");   

            await transaction.ExecuteAsync(CommandFlags.FireAndForget);
            return Unit.Value;
        }
    }
}