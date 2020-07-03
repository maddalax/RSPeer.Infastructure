using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Application.Features.Scripts.Commands.ConfirmScript;
using RSPeer.Application.Features.Scripts.Commands.CreateScript;
using RSPeer.Application.Features.Scripts.Commands.DeleteScript;
using RSPeer.Application.Features.Scripts.Commands.DenyScript;
using RSPeer.Application.Features.Scripts.Queries.GetScript;
using RSPeer.Application.Features.UserManagement.Users.Queries;

namespace RSPeer.Api.Controllers.Admin
{
	[Owner]
	public class AdminScriptController : BaseController
	{
		private readonly IMediator _mediator;

		public AdminScriptController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		public async Task<IActionResult> Update([FromBody] CreateScriptCommand command)
		{
			var admin = await GetUser();
			command.Admin = admin;
			var script = await _mediator.Send(new GetFullScriptByIdQuery { ScriptId = command.Script.Id });
			var user = await _mediator.Send(new GetUserByIdQuery { Id = script.UserId });
			command.User = user;
			var request = await Mediator.Send(command);
			await Mediator.Send(new ConfirmScriptCommand
			{
				User = admin,
				ScriptId = request
			});
			await _mediator.Send(new RemovePendingRequestsCommand { ScriptId = command.Script.Id });
			return Ok();
		}
		
		[HttpPost]
		[Owner]
		public async Task<IActionResult> Deny([FromQuery] string reason, int scriptId)
		{
			var user = await GetUser();
			await _mediator.Send(new DenyScriptCommand
			{
				Admin = user,
				Reason = reason,
				ScriptId = scriptId
			});
			return Ok();
		}

		[HttpPost]
		public async Task<IActionResult> Delete([FromQuery] int id)
		{
			var user = await GetUser();
			return Ok(await _mediator.Send(new DeleteScriptCommand { ScriptId = id, Admin = user }));
		}
	}
}