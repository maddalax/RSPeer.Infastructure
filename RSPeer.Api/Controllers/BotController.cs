using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Features.Bot.Queries;
using RSPeer.Application.Features.Files.Queries;
using RSPeer.Application.Features.Files.Utility;
using Game = RSPeer.Domain.Entities.Game;

namespace RSPeer.Api.Controllers
{
	public class BotController : BaseController
	{
		public async Task<IActionResult> CurrentVersion([FromQuery] Game game = Game.Osrs)
		{
			var file = FileHelper.GetFileName(game);
			var version = await Mediator.Send(new GetLatestFileVersionQuery { Name = file });
			return Ok(new {version});
		}
		
		public async Task<IActionResult> CurrentVersionRaw([FromQuery] Game game = Game.Osrs)
		{
			var file = FileHelper.GetFileName(game);
			return Ok(await Mediator.Send(new GetLatestFileVersionQuery { Name = file }));
		}
		
		public async Task<IActionResult> CurrentVersionNew([FromQuery] Game game = Game.Osrs)
		{
			return await CurrentVersionRaw(game);
		}
		
		public async Task<IActionResult> GetVersionByHash([FromQuery] string hash, Game game = Game.Osrs)
		{
			return Ok(await Mediator.Send(new GetVersionByFileHashQuery {Hash = hash, Game = game}));
		}

		[RateLimit(150, 1)]
		public async Task<IActionResult> CurrentJar([FromQuery] Game game = Game.Osrs)
		{
			var file = FileHelper.GetFileName(game);

			if (game == Game.Rs3)
			{
				var inuvation = await Mediator.Send(new GetInuvationJarQuery { UserId = await GetUserIdOrThrow() });
				return File(inuvation, "application/java-archive", file);
			}

			var jar = await Mediator.Send(new GetFileQuery { Name = file });
			return File(jar, "application/java-archive", file);
		}
	
		[NotDisabled]
		[RateLimit(150, 1)]
		public async Task<IActionResult> CurrentModscript()
		{
			var jar = await Mediator.Send(new GetFileQuery { Name = "bot-modscript" });
			return File(jar, "text/plain", "juice");
		}
	}
}