using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.Migration;
using RSPeer.Api.Middleware.ModelBinders;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Features.ScriptAccesses.Commands;
using RSPeer.Application.Features.ScriptAccesses.Queries;
using RSPeer.Application.Features.Scripts.Commands.ConfirmScript;
using RSPeer.Application.Features.Scripts.Commands.CreateScript;
using RSPeer.Application.Features.Scripts.Commands.DeleteScript;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Dtos;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers
{
	public class ScriptController : BaseController
	{
		[HttpPost]
		[RateLimit(100, 1)]
		public async Task<ActionResult<IEnumerable<ScriptDto>>> List([FromBody] GetScriptsQuery query)
		{
			if (query == null)
			{
				return Ok(new List<ScriptDto>());
			}
			if (query.Type == ScriptType.Mine || query.Type == ScriptType.Private)
			{
				query.UserId = (await GetUser())?.Id;
			}
			var scripts = await Mediator.Send(query);
			return Ok(scripts);
		}

		[HttpGet]
		[RateLimit(100, 1)]
		public async Task<ActionResult<ScriptDto>> GetScriptById([FromQuery] int id)
		{
			var script = await Mediator.Send(new GetScriptByIdQuery { ScriptId = id, IncludeDeveloper = true});
			return Ok(script);
		}

		[HttpGet]
		[LoggedIn]
		[RateLimit(100, 1)]
		public async Task<ActionResult<IEnumerable<int>>> AccessIds()
		{
			var user = await GetUser();
			return Ok(await Mediator.Send(new GetAccessScriptIdsQuery
			{
				UserId = user.Id
			}));
		}
		
		[HttpPost]
		[LoggedIn]
		[RateLimit(50, 1)]
		public async Task<ActionResult<IEnumerable<ScriptAccess>>> ListAccess([FromBody] GetScriptAccessQuery query)
		{
			var user = await GetUser();
			query.UserId = user.Id;
			var accesses = await Mediator.Send(query);
			return Ok(accesses.Select(w => new ScriptAccessDto(w)));
		}

		[HttpPost]
		[LoggedIn]
		[RateLimit(100, 1)]
		public async Task<IActionResult> RemoveAccess(RemoveScriptAccessCommand command)
		{
			var user = await GetUser();
			command.UserId = user.Id;
			return Ok(await Mediator.Send(command));
		}
		
		[HttpPost]
		[LoggedIn]
		[RateLimit(100, 1)]
		[ReadOnly]
		public async Task<IActionResult> AddAccess(AddScriptAccessCommand command)
		{
			var user = await GetUser();
			command.UserId = user.Id;
			command.AdminUserId = null;
			return Ok(await Mediator.Send(command));
		}

		[HttpPost]
		[LoggedIn]
		[RateLimit(10, 1)]
		public async Task<ActionResult<int>> Create([FromBody] CreateScriptCommand command)
		{
			command.User = await GetUser();
			return Ok(await Mediator.Send(command));
		}

		[HttpPost]
		[Owner]
		[RateLimit(10, 1)]
		public async Task<ActionResult<int>> CreatePublicHidden(
			[ModelBinder(BinderType = typeof(JsonModelBinder))] CreatePublicHiddenScriptCommand command, 
			IFormFile file)
		{
			command.User = await GetUser();
			command.File = file;
			return Ok(await Mediator.Send(command));
		}
		
		[HttpPost]
		[LoggedIn]
		[RateLimit(10, 1)]
		public async Task<ActionResult<int>> CreatePrivate(
			[ModelBinder(BinderType = typeof(JsonModelBinder))] CreatePrivateScriptCommand command, 
			IFormFile file)
		{
			command.User = await GetUser();
			command.File = file;
			return Ok(await Mediator.Send(command));
		}
		
		[HttpPost]
		[LoggedIn]
		[RateLimit(10, 1)]
		public async Task<ActionResult<int>> DeletePrivate(
			DeletePrivateScriptCommand command)
		{
			command.UserId = (await GetUser()).Id;
			return Ok(await Mediator.Send(command));
		}
		
		[HttpGet]
		[LoggedIn]
		[RateLimit(100, 1)]
		public async Task<ActionResult<IEnumerable<ScriptDto>>> ListPrivate()
		{
			var user = await GetUser();
			var scripts = await Mediator.Send(new GetScriptersPrivateScriptsQuery { UserId = user.Id });
			return Ok(scripts.Select(s => new ScriptDto(s)));
		}
		
		[HttpPost]
		[LoggedIn]
		[RateLimit(200, 1)]
		public async Task<IActionResult> AddPrivateAccess([FromBody] AddPrivateScriptAccessCommand command)
		{
			command.RequestingUserId = (await GetUser()).Id;
			return Ok(await Mediator.Send(command));
		}
		
		[HttpPost]
		[LoggedIn]
		[RateLimit(200, 1)]
		public async Task<IActionResult> RevokePrivateAccess([FromBody] RemovePrivateScriptAccessCommand command)
		{
			command.RequestingUserId = (await GetUser()).Id;
			return Ok(await Mediator.Send(command));
		}


			[HttpPost]
		[Owner]
		public async Task<ActionResult<int>> Confirm([FromBody] ConfirmScriptCommand command)
		{
			command.User = await GetUser();
			return Ok(await Mediator.Send(command));
		} 

		[HttpGet]
		[RateLimit(100, 1)]
		public async Task<ActionResult<Script>> Content([FromQuery] int id)
		{
			var query = new GetScriptContentBytesQuery { ScriptId = id, User = await GetUser(), CheckAccess = true };
			var file = await Mediator.Send(query);
			return new FileContentResult(file, "application/java-archive");
		}

		[HttpGet]
		[RateLimit(100, 1)]
		public IActionResult Categories()
		{
			return Ok(Enum.GetValues(typeof(ScriptCategory)).Cast<ScriptCategory>().Select(w => w.ToString()));
		}
	}
}