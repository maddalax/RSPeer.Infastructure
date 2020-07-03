using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Application.Features.Forums.Discourse.Commands;
using RSPeer.Application.Features.ScriptAccesses.Queries;
using RSPeer.Application.Features.UserManagement.Users.Commands;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers.Admin
{
	[Owner]
	public class AdminUserController : BaseController
	{
		public async Task<ActionResult<IEnumerable<User>>> Search([FromQuery] string term)
		{
			return Ok(await Mediator.Send(new SearchUsersQuery { SearchTerm = term }));
		}

		[HttpPost]
		public async Task<IActionResult> Disable([FromBody] UserDisableCommand command)
		{
			return Ok(await Mediator.Send(command));
		}

		[HttpPost]
		public async Task<IActionResult> UpdateBalance([FromBody] UserUpdateBalanceCommand command)
		{
			command.AdminUserId = (await GetUser()).Id;
			return Ok(await Mediator.Send(command));
		}

		[HttpPost]
		public async Task<ActionResult<IEnumerable<BalanceChange>>> BalanceChanges([FromBody] GetUserBalanceChangesQuery query)
		{
			return Ok(await Mediator.Send(query));
		}
		
		[HttpPost]
		public async Task<ActionResult<IEnumerable<ScriptAccessDto>>> ScriptAccess([FromBody] GetScriptAccessQuery query)
		{
			var accesses = await Mediator.Send(query);
			return Ok(accesses.Select(w => new ScriptAccessDto(w)).OrderBy(w => w.Script.Name));
		}
		
		[HttpPost]
		public async Task<ActionResult<IEnumerable<BalanceChange>>> IpAccess([FromBody] GetIpAddressAccessQuery query)
		{
			return Ok(await Mediator.Send(query));
		}
		
		[HttpGet]
		public async Task<IActionResult> SyncDiscourse([FromQuery] string email)
		{
			return Ok(await Mediator.Send(new SyncDiscourseUsersCommand {Email = email}));
		}
	}
}