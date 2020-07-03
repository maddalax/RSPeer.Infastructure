using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Application.Features.SiteConfig.Commands;

namespace RSPeer.Api.Controllers.Admin
{
	[Owner]
	public class AdminConfigController : BaseController
	{
		[HttpPost]
		public async Task<IActionResult> SetConfig([FromBody] SetSiteConfigCommand command)
		{
			return Ok(await Mediator.Send(command));
		}
	}
}