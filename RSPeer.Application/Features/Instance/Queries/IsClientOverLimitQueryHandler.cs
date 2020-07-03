using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Instance.Queries
{
    public class IsClientOverLimitQueryHandler : IRequestHandler<IsClientOverLimitQuery, bool>
    {
        private readonly IRedisService _redis;
        private readonly ILogger<IsClientOverLimitQueryHandler> _logger;

        public IsClientOverLimitQueryHandler(IRedisService redis, ILogger<IsClientOverLimitQueryHandler> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<bool> Handle(IsClientOverLimitQuery request, CancellationToken cancellationToken)
        {
            var key = $"clients_over_limit_{request.Tag}";

            try
            {
                var over = await _redis.GetDatabase().StringGetAsync(key);
                
                if (over.HasValue)
                {
                    if(int.TryParse(over.ToString(), out var parsed))
                    {
                        return parsed != 0;
                    }

                    return bool.Parse(over.ToString());
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, "Failed to check instance limit for " + request.UserId + " " + request.Tag);
                return false;
            }

            return false;
        }
    }
}