using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Instance.Queries
{
    public class GetRunningClientsQueryHandler : IRequestHandler<GetRunningClientsQuery, IEnumerable<RunescapeClient>>
    {
        private readonly IRedisService _redis;

        public GetRunningClientsQueryHandler(IRedisService redis)
        {
            _redis = redis;
        }

        public async Task<IEnumerable<RunescapeClient>> Handle(GetRunningClientsQuery request,
            CancellationToken cancellationToken)
        {
            var tags = await _redis.GetSet($"{request.UserId}_running_client");
            if (tags.Count == 0)
            {
                return new List<RunescapeClient>();
            }
            var keys = tags.Select(w => (RedisKey) $"{w}_client_details").ToArray();
            var clients = await _redis.GetDatabase().StringGetAsync(keys);
            return clients.Where(w => w.HasValue).Select(w => JsonSerializer.Deserialize<RunescapeClient>(w));
        }
    }
}