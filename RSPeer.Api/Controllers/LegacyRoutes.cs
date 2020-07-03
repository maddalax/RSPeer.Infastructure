using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Application.Features.Bot.Queries;
using RSPeer.Application.Features.Files.Queries;
using RSPeer.Application.Features.UserManagement.Users.Commands;

namespace RSPeer.Api.Controllers
{
	public class LegacyRoutes : BaseLegacyController
	{
		[Route("bot/currentJar")]
		public async Task<IActionResult> CurrentJar()
		{
			var jar = await Mediator.Send(new GetFileQuery { Name = "rspeer.jar" });
			return File(jar, "application/java-archive", "rspeer.jar");
		}

		[Route("bot/currentVersionNew")]
		public async Task<IActionResult> CurrentVersionNew()
		{
			return Ok(await Mediator.Send(new GetLatestFileVersionQuery { Name = "rspeer.jar" }));
		}

		[Route("user/login")]
		public async Task<IActionResult> Login(UserSignInCommand command)
		{
			var result = await Mediator.Send(command);
			return Ok(new { accessToken = result.Token, idToken = result.Token });
		}

		[Route("bot/currentHash")]
		public async Task<IActionResult> CurrentHash()
		{
			var hash = await Mediator.Send(new GetLatestBotVersionHashQuery());
			return Ok(hash);
		}

	}
}