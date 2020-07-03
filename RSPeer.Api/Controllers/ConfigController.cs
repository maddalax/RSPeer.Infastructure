using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Application.Features.SiteConfig.Queries;

namespace RSPeer.Api.Controllers
{
	public class ConfigController : BaseController
	{
		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] string key)
		{
			return Ok(await Mediator.Send(new GetSiteConfigOrThrowCommand { Key = key }));
		}
	}
}