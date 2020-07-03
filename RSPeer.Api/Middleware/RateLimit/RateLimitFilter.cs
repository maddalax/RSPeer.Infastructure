using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using RSPeer.Api.Utilities;
using RSPeer.Application.Infrastructure.Caching.Base;

namespace RSPeer.Api.Middleware.RateLimit
{
    public class RateLimitFilter : IAsyncAuthorizationFilter
    {
        private readonly IRedisService _redis;
        private readonly int _lengthInMinutes;
        private readonly int _maxRequests;
        private readonly ILogger<RateLimitFilter> _logger;

        public RateLimitFilter(int maxRequests, int lengthInMinutes, IRedisService redis, ILogger<RateLimitFilter> logger)
        {
            _redis = redis;
            _maxRequests = maxRequests;
            _lengthInMinutes = lengthInMinutes;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var path = context.HttpContext.Request.Path.Value;
            var ip = HttpUtilities.GetIp(context.HttpContext);
            var key = $"{ip}_{path}_rl";

            var result = await _redis.Increment(key);
            var ttl = await _redis.TimeToLive(key);

            if (!ttl.HasValue)
            {
                await _redis.KeyExpire(key, TimeSpan.FromSeconds(60));
            }
            
            if (result >= _maxRequests * 2)
            {
                _logger.LogWarning("Rate limited " + ip + " on " + path + " with attempts " + result + ".");
                context.Result = new StatusCodeResult((int) HttpStatusCode.TooManyRequests);
            }
        }
    }
}