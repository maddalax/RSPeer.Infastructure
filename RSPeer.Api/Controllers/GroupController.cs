using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.Migration;
using RSPeer.Application.Features.Groups.Commands;
using RSPeer.Application.Features.Groups.Queries;

namespace RSPeer.Api.Controllers
{
	public class GroupController : BaseController
	{
		[HttpPost]
		[Owner]
		[ReadOnly]
		public async Task<ActionResult<int>> Add([FromBody] AddGroupCommand command)
		{
			return Ok(await Mediator.Send(command));
		}

		[HttpGet]
		[Owner]
		public async Task<ActionResult<int>> List()
		{
			return Ok(await Mediator.Send(new GetGroupsQuery()));
		}
	}
}