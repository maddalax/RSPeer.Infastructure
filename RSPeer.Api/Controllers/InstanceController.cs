using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Features.Instance.Queries;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers
{
	public class InstanceController : BaseController
	{
		[HttpGet]
		[LoggedIn]
		[RateLimit(150, 1)]
		public async Task<ActionResult<IEnumerable<RunescapeClient>>> RunningClients()
		{
			var user = await GetUser();
			return Ok(await Mediator.Send(new GetRunningClientsQuery { UserId = user.Id }));
		}

		[HttpGet]
		[LoggedIn]
		[RateLimit(1000, 1)]
		public async Task<IActionResult> Limits()
		{
			var user = await GetUser();
			var allowed = await Mediator.Send(new GetAllowedInstancesQuery { UserId = user.Id });
			var current = await Mediator.Send(new GetRunningClientsCountQuery { UserId = user.Id });
			return Ok(new BotInstanceResult
			{
				CanRunMore = allowed > current,
				Allowed = allowed,
				NewCount = current,
				Running = current
			});
		}

		[HttpGet]
		[LoggedIn]
		[RateLimit(150, 1)]
		public async Task<ActionResult<int>> AllowedClients()
		{
			var user = await GetUser();
			return Ok(await Mediator.Send(new GetAllowedInstancesQuery { UserId = user.Id }));
		}

		[HttpGet]
		[LoggedIn]
		[RateLimit(150, 1)]
		public async Task<ActionResult<int>> RunningClientsCount()
		{
			var user = await GetUser();
			return Ok(await Mediator.Send(new GetRunningClientsCountQuery { UserId = user.Id }));
		}
	}
}