using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Application.Features.Stats.Models;
using RSPeer.Application.Features.Stats.Queries;

namespace RSPeer.Api.Controllers.Admin
{
	[Owner]
	public class AdminStatsController : BaseController
	{
		public async Task<ActionResult<IEnumerable<UserAndCountResult>>> Top25()
		{
			return Ok(await Mediator.Send(new GetTop25UsersRunningQuery()));
		}
	}
}