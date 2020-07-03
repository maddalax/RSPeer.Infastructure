using System;
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

namespace RSPeer.Application.Features.Launchers.Queries
{
    public class GetRegisteredLaunchersQueryHandler : IRequestHandler<GetRegisteredLaunchersQuery, IEnumerable<Domain.Entities.Launcher>>
    {
        private readonly IRedisService _redis;

        public GetRegisteredLaunchersQueryHandler(IRedisService redis)
        {
            _redis = redis;
        }

        public async Task<IEnumerable<Launcher>> Handle(GetRegisteredLaunchersQuery request, CancellationToken cancellationToken)
        {
            var launcherIds = await _redis.GetSet($"{request.UserId}_connected_launchers");
            if (!launcherIds.Any())
            {
                return new List<Launcher>();
            }

            var keys = launcherIds.Select(w => (RedisKey) $"{request.UserId}_{w}_launcher").ToArray();
            var launchers = await _redis.GetDatabase().StringGetAsync(keys);    
            var currentLaunchers = launchers.Where(w => w.HasValue).Select(w => JsonSerializer.Deserialize<Launcher>(w));
            var currentLauncherIds = currentLaunchers.Select(w => w.Tag).ToHashSet();
            var toRemove = launcherIds.Where(w => !currentLauncherIds.Contains(Guid.Parse(w.ToString())));
                
            if (toRemove.Any())
            {
                var transaction = _redis.GetDatabase().CreateTransaction();
                foreach (var value in toRemove)
                {
                    transaction.SetRemoveAsync($"{request.UserId}_connected_launchers", value);
                }

                await transaction.ExecuteAsync(CommandFlags.FireAndForget);
            }

            return currentLaunchers;
        }
    }
}