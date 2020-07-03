using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Dtos;
using RSPeer.Persistence;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Scripts.Queries.GetScript
{
    public class GetScriptsByIdsQueryHandler : IRequestHandler<GetScriptsByIdsQuery, IEnumerable<ScriptDto>>
    {
        private readonly IRedisService _redis;
        private readonly RsPeerContext _db;

        public GetScriptsByIdsQueryHandler(IRedisService redis, RsPeerContext db)
        {
            _redis = redis;
            _db = db;
        }

        public async Task<IEnumerable<ScriptDto>> Handle(GetScriptsByIdsQuery request, CancellationToken cancellationToken)
        {
            var keys = request.ScriptIds.Select(w => (RedisKey) $"script_{w}_details").ToArray();
            var scripts = await _redis.GetDatabase().StringGetAsync(keys);
            var toSet = new HashSet<int>(request.ScriptIds);
            var results = new List<ScriptDto>();
            foreach (var value in scripts)
            {
                if (!value.HasValue) continue;
                var script = JsonSerializer.Deserialize<ScriptDto>(value.ToString());
                toSet.Remove(script.Id);
                results.Add(script);
            }

            if (toSet.Count > 0)
            {
                var list = toSet.ToList();
                var toSetScripts = await _db.Scripts.Where(w => list.Contains(w.Id))
                    .Include(w => w.User)
                    .ToListAsync(cancellationToken);
                results.AddRange(toSetScripts.Select(w => new ScriptDto(w)));
                var transaction = _redis.GetDatabase().CreateTransaction();
                foreach (var script in toSetScripts)
                {
                    transaction.StringSetAsync($"script_{script.Id}_details", JsonSerializer.Serialize(new ScriptDto(script)));
                }

                await transaction.ExecuteAsync(CommandFlags.FireAndForget);
            }

            return results;
        }
    }
}