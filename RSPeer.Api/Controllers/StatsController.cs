using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Api.Controllers
{
	public class StatsController : BaseController
	{
		private readonly IRedisService _redis;
		private readonly RsPeerContext _db;

		public StatsController(IRedisService redis, RsPeerContext db)
		{
			_redis = redis;
			_db = db;
		}

		[HttpGet]
		[RateLimit(50, 1)]
		public async Task<IActionResult> Connected()
		{
			return Ok(await _redis.Get<long>("connected_client_count"));
		}
		
		[HttpGet]
		[RateLimit(50, 1)]
		public async Task<IActionResult> TotalUsers()
		{
			var count = await _redis.GetOrDefault("total_users_count", async () => await _db.Users.CountAsync(), TimeSpan.FromHours(1));
			return Ok(count);
		}
	}
}