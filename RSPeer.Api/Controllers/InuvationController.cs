using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Features.Bot.Commands.Inuvation;

namespace RSPeer.Api.Controllers
{
    public class InuvationController : BaseController
    {
        [RateLimit(150, 1)]
        [HttpPost]
        [LoggedIn]
        public async Task<IActionResult> Modscript([FromBody] GenerateModscriptCommand command)
        {
            command.UserId = await GetUserIdOrThrow();
            var jar = await Mediator.Send(command);
            return File(jar, "text/plain", "juice");
        }
    }
}