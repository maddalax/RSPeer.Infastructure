using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.Migration;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Api.Utilities;
using RSPeer.Application.Features.UserData.Commands;
using RSPeer.Application.Features.UserLogging.Commands;
using RSPeer.Application.Features.UserManagement.Users.Commands;
using RSPeer.Application.Features.UserManagement.Users.Models;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Application.Features.UserManagement.Users.Queries.Options;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers
{
    public class UserController : BaseController
    {
        [HttpGet]
        [Owner]
        public async Task<ActionResult<int>> ById([FromQuery] int id, bool groups)
        {
            return Ok(await Mediator.Send(new GetUserByIdQuery
            {
                Id = id,
                IncludeGroups = groups
            }));
        }

        [HttpPost]
        [LoggedIn]
        public async Task<ActionResult<User>> Me([FromBody] UserLookupOptions options, [FromQuery] bool full)
        {
            options ??= new UserLookupOptions();
            options.FullUser = full || options.IncludeBalance;
            options.IncludeBalance = options.IncludeBalance || options.FullUser;
            var user = await GetUser(options);
            user = options.IncludeBalance
                ? await Mediator.Send(new GetUserByIdQuery
                    {Id = user.Id, IncludeGroups = options.FullUser, IncludeInstances = options.FullUser})
                : user;
            return Ok(user);
        }

        [HttpGet]
        [LoggedIn]
        public async Task<ActionResult<User>> Me()
        {
            var user = await GetUser();
            return Ok(user);
        }

        [HttpPost]
        [RateLimit(5, 1)]
        [ReadOnly]
        public async Task<ActionResult<int>> SignUp([FromBody] UserSignUpCommand command)
        {
            return Ok(new {Id = await Mediator.Send(command)});
        }

        [HttpPost]
        [RateLimit(5, 1)]
        public async Task<ActionResult<UserSignInResult>> SignIn([FromBody] UserSignInCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost]
        [RateLimit(5, 1)]
        public async Task<ActionResult<UserSignInResult>> Login([FromBody] UserSignInCommand command)
        {
            return await SignIn(command);
        }

        [HttpPost]
        [RateLimit(10, 1)]
        public async Task<ActionResult<UserSignInResult>> ResetPass([FromQuery] string email)
        {
            return Ok(await Mediator.Send(new UserResetPasswordCommand {Email = email}));
        }

        [HttpPost]
        [RateLimit(10, 1)]
        public async Task<IActionResult> ResetPassConfirm([FromBody] UserResetPasswordConfirmCommand confirm)
        {
            return Ok(await Mediator.Send(confirm));
        }

        [HttpPost]
        [Owner]
        [ReadOnly]
        public async Task<ActionResult<int>> UpdateBalance([FromBody] UserUpdateBalanceCommand command)
        {
            return Ok(new {Balance = await Mediator.Send(command)});
        }

        [HttpGet]
        [LoggedIn]
        [RateLimit(100, 1)]
        public async Task<ActionResult<IEnumerable<BalanceChange>>> BalanceChanges()
        {
            var user = await GetUser();
            return Ok(await Mediator.Send(new GetUserBalanceChangesQuery {UserId = user.Id}));
        }

        [HttpGet]
        [RateLimit(250, 1)]
        public async Task<ActionResult<string>> ByDiscord([FromQuery] string id)
        {
            var account = await Mediator.Send(new GetDiscordAccountQuery {DiscordId = id});
            var user = account == null ? null : await Mediator.Send(new GetUserByIdQuery {Id = account.UserId});
            return Ok(new {username = user?.Username});
        }

        [HttpGet]
        [RateLimit(250, 1)]
        public async Task<ActionResult<string>> GetDiscordId([FromQuery] string username)
        {
            var account = await Mediator.Send(new GetDiscordAccountQuery {Username = username});
            return Ok(new {id = account?.DiscordUserId});
        }

        [HttpGet]
        [RateLimit(1000, 1)]
        [LoggedIn]
        public async Task<ActionResult<UserClientConfig>> GetClientConfig()
        {
            var user = await GetUser();
            var config = await Mediator.Send(new GetOrSetUserClientConfigCommand {UserId = user.Id});
            return Ok(config);
        }
        
        [HttpPost]
        [RateLimit(500, 1)]
        public IActionResult Log()
        {
            // Disabled for now.
            /*
            var user = await GetUser();
            command.UserId = user?.Id ?? -1;
            await Mediator.Send(command);
            */
            return NoContent();
        }

        [HttpGet]
        public ActionResult<UserSignInResult> ConvertToken()
        {
            var session = HttpUtilities.TryGetSession(HttpContext);
            return Ok(!string.IsNullOrEmpty(session)
                ? new UserSignInResult {Token = session}
                : new UserSignInResult {Token = null});
        }
    }
}