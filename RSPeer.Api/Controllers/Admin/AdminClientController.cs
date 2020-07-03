using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Application.Features.Instance.Queries;

namespace RSPeer.Api.Controllers.Admin
{
	[Owner]
	public class AdminClientController : BaseController
	{
		[HttpGet]
		public async Task<IActionResult> RunningClients([FromQuery] int userId)
		{
			return Ok(await Mediator.Send(new GetRunningClientsQuery { OrderByDate = true, UserId = userId }));
		}
	}
}