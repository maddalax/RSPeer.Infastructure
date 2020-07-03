using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Messaging.Queries
{
    public class GetRemoteMessagesQueryHandler : IRequestHandler<GetRemoteMessagesQuery, IEnumerable<RemoteMessage>>
    {
        private readonly IRedisService _redis;

        public GetRemoteMessagesQueryHandler(IRedisService redis)
        {
            _redis = redis;
        }

        public async Task<IEnumerable<RemoteMessage>> Handle(GetRemoteMessagesQuery request, CancellationToken cancellationToken)
        {
            var key = $"messages_{request.UserId}";
            var messageIds = await _redis.GetSet(key);

            if (!messageIds.Any())
            {
                return new List<RemoteMessage>();
            }
            
            var keys = messageIds.Select(w => (RedisKey) $"messages_{request.UserId}_{w}").ToArray();
            var values = await _redis.GetDatabase().StringGetAsync(keys);
            var messages = values.Where(w => w.HasValue).Select(w => JsonSerializer.Deserialize<RemoteMessage>(w));
            
            return messages.Where(w => w.Consumer == request.Consumer);
        }
    }
}