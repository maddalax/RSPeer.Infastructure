using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.Migration;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Api.Utilities;
using RSPeer.Application.Features.BotPanel;
using RSPeer.Application.Features.Launchers.Commands;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers
{
	[LoggedIn]
	public class BotLauncherController : BaseController
	{
		private readonly IBotLauncherService _service;

		public BotLauncherController(IBotLauncherService service)
		{
			_service = service;
		}

		[HttpPost]
		[RateLimit(150, 1)]
		public async Task<IActionResult> Register([FromBody] RegisterLauncherCommand command)
		{
			var session = HttpUtilities.TryGetSession(Request.HttpContext);
			if (session != null)
			{
				var userId = await Mediator.Send(new GetUserIdBySessionQuery {Session = session});
				command.UserId = userId;
			}

			await Mediator.Send(command);
			return NoContent();
		}
		
		
		[HttpPost]
		[RateLimit(150, 1)]
		public async Task<IActionResult> Unregister([FromBody] UnregisterLauncherCommand command)
		{
			command.UserId = await GetUserId() ?? -1;
			await Mediator.Send(command);
			return NoContent();
		}

		[HttpGet]
		[RateLimit(150, 1)]
		public async Task<IActionResult> Connected()
		{
			var launchers = await _service.GetConnectedLaunchers(await GetUserIdOrThrow());
			return Ok(launchers);
		}

		[HttpPost]
		[RateLimit(150, 1)]
		public async Task<IActionResult> Send([FromBody] JObject body)
		{
			return Ok(await _service.SendJson(body, await GetUserIdOrThrow()));
		}
		
		
		[HttpPost]
		[RateLimit(150, 1)]
		public async Task<IActionResult> SendNew([FromQuery] string message, string tag = null)
		{
			await _service.Send(message, await GetUserIdOrThrow(), tag);
			return Ok();
		}
		
		[HttpPost]
		[RateLimit(100, 1)]
		public async Task<IActionResult> Kill([FromQuery] string id)
		{
			return Ok(await _service.Kill(id, await GetUserIdOrThrow()));
		}

		[HttpGet]
		[RateLimit(150, 1)]
		public async Task<IActionResult> ConnectedClients()
		{
			return Ok(await _service.ConnectedClients(await GetUserIdOrThrow()));
		}

		[HttpGet]
		[RateLimit(250, 1)]
		public async Task<IActionResult> Logs([FromQuery] string id, string top, string skip, string type)
		{
			return Ok(await _service.GetLogs(await GetUserIdOrThrow(), id, top, skip, type));
		}

		[HttpGet]
		[RateLimit(100, 1)]
		public async Task<IActionResult> GetKey()
		{
			return Ok(await _service.GetKey(await GetUserIdOrThrow()));
		}

		[HttpPost]
		[RateLimit(10, 1)]
		[ReadOnly]
		public async Task<IActionResult> UpdateKey()
		{
			return Ok(await _service.ChangeKey(await GetUserIdOrThrow()));
		}

		[HttpPost]
		[ReadOnly]
		[RateLimit(50, 1)]
		public async Task<IActionResult> SaveQuickLaunch([FromBody] QuickLaunch launch)
		{
			await _service.SaveQuickLaunch(await GetUserIdOrThrow(), launch);
			return Ok();
		}

		[HttpGet]
		[RateLimit(100, 1)]
		public async Task<IActionResult> GetQuickLaunch()
		{
			var result = await _service.GetQuickLaunch(await GetUserIdOrThrow());
			return Ok(result);
		}

		[HttpPost]
		[ReadOnly]
		[RateLimit(100, 1)]
		public async Task<IActionResult> DeleteQuickLaunch([FromQuery] string id)
		{
			await _service.DeleteQuickLaunch(await GetUserIdOrThrow(), id);
			return Ok();
		}

		[HttpPost]
		[ReadOnly]
		[RateLimit(100, 1)]
		public async Task<IActionResult> SaveProxy([FromBody] RsClientProxy proxy)
		{
			await _service.SaveProxy(await GetUserIdOrThrow(), proxy);
			return Ok();
		}

		[HttpPost]
		[ReadOnly]
		[RateLimit(100, 1)]
		public async Task<IActionResult> DeleteProxy([FromQuery] string id)
		{
			await _service.DeleteProxy(await GetUserIdOrThrow(), id);
			return Ok();
		}

		[HttpGet]
		[RateLimit(100, 1)]
		public async Task<IActionResult> GetProxies()
		{
			return Ok(await _service.GetProxies(await GetUserIdOrThrow()));
		}
	}
}