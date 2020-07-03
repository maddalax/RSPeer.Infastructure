using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Migration.Queries
{
    public class IsReadOnlyModeQueryHandler : IRequestHandler<IsReadOnlyModeQuery, bool>
    {
        private readonly IRedisService _redis;
        private readonly RsPeerContext _db;

        public IsReadOnlyModeQueryHandler(IRedisService redis, RsPeerContext db)
        {
            _redis = redis;
            _db = db;
        }

        public async Task<bool> Handle(IsReadOnlyModeQuery request, CancellationToken cancellationToken)
        {
            return await _redis.GetOrDefault("migration:readonly:enabled", async () =>
            {
                var config = await _db.SiteConfig.FirstOrDefaultAsync(w => w.Key == "migration:readonly:enabled", cancellationToken);
				
                if (config == null)
                {
                    return false;
                }

                return bool.TryParse(config.Value, out var result) && result;
            }, TimeSpan.FromMinutes(1));
        }
    }
}