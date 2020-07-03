using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Utilities;
using RSPeer.Persistence;

namespace RSPeer.Api.Controllers
{
	public class HealthController : BaseController
	{
		private readonly RsPeerContext _db;

		public HealthController(RsPeerContext db)
		{
			_db = db;
		}
		[HttpGet]
		public IActionResult Status()
		{
			return Ok();
		}

		[HttpGet]
		public async Task<IActionResult> Message()
		{
			var enabled = await _db.SiteConfig.FirstOrDefaultAsync(w => w.Key == "status:message:enabled");
			if (enabled == null || !bool.Parse(enabled.Value))
			{
				return Ok();
			}
			var message = await _db.SiteConfig.FirstOrDefaultAsync(w => w.Key == "status:message");
			return Ok(message?.Value ?? string.Empty);
		}

		[HttpGet]
		public IActionResult Meta()
		{
			return Ok(new
			{
				Environment.MachineName, Program.Version
			});
		}


		[HttpGet]
		public IActionResult Ip()
		{
			return Ok(JsonConvert.SerializeObject(Environment.GetEnvironmentVariables()));
		}
	}
}