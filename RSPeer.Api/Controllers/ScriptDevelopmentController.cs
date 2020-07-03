using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.Migration;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Features.Scripts.Commands.CreateScript;
using RSPeer.Application.Features.Scripts.Queries.CreateScript;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Domain.Dtos;
using RSPeer.Domain.Entities;
using RSPeer.Infrastructure.Gitlab.Entities;

namespace RSPeer.Api.Controllers
{
	public class ScriptDevelopmentController : BaseController
	{
		[HttpGet]
		[LoggedIn]
		[RateLimit(100, 1)]
		public async Task<ActionResult<IEnumerable<Script>>> ListForScripter()
		{
			var user = await GetUser();
			var scripts = await Mediator.Send(new GetScriptsForScripterQuery { UserId = user.Id });
			return Ok(scripts.Select(w => new ScriptDto(w)));
		}

		[HttpGet]
		[LoggedIn]
		[RateLimit(50, 1)]
		public async Task<ActionResult<ScripterInfo>> ScripterInfo()
		{
			var user = await GetUser();
			return Ok(await Mediator.Send(new GetScripterInfoQuery { UserId = user.Id }));
		}

		[HttpPost]
		[LoggedIn]
		[RateLimit(50, 1)]
		[ReadOnly]
		public async Task<ActionResult<ScripterInfo>> AddScripterInfo([FromBody] AddScripterInfoCommand command)
		{
			var user = await GetUser();
			command.UserId = user.Id;
			return Ok(await Mediator.Send(command));
		}

		[HttpPost]
		[LoggedIn]
		[RateLimit(50, 1)]
		public async Task<ActionResult<IEnumerable<GitlabUser>>> QueryGitLab([FromBody] QueryGitlabUsersCommand command)
		{
			return Ok(await Mediator.Send(command));
		}
		
		[HttpGet]
		[LoggedIn]
		[RateLimit(100, 1)]
		public async Task<ActionResult<IEnumerable<GitlabUser>>> GetMessages()
		{
			var user = await GetUser();
			return Ok(await Mediator.Send(new GetPendingScriptMessagesQuery {User = user}));
		}
	}
}