using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Application.Features.Scripter.Payout.Commands;
using RSPeer.Application.Features.Scripter.Payout.Queries;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers.Admin
{
	[Owner]
	public class AdminScripterPayoutController : BaseController
	{
		public async Task<IActionResult> Calculate()
		{
			var data = await Mediator.Send(new GetScriptersToPayoutQuery());
			return Ok(data.ToList());
		}

		public async Task<IActionResult> Complete([FromBody] ScripterPayout payout)
		{
			var command = new CompleteScripterPayoutCommand {Payout = payout, Admin = await GetUser()};
			await Mediator.Send(command);
			return Ok();
		}
	}
}