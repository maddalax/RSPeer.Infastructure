using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Features.WebWalker.Queries;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers
{
    [LoggedIn]
    public class WalkerController : BaseController
    {
        private readonly IMediator _mediator;

        public WalkerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [RateLimit(100, 1)]
        public async Task<IActionResult> GenerateBankPaths([FromBody] JObject payload, [FromQuery] WebWalker? walker)
        {
            var result = await _mediator.Send(new GetWebPathQuery
            {
                UserId = await GetUserIdOrThrow(),
                Payload = payload,
                Type = WebPathType.Bank,
                WebWalker = walker
            });
            Request.Headers["X-Web-Walker"] = result.Walker.ToString();
            return Ok(result.Result);
        }

        [RateLimit(100, 1)]
        public async Task<IActionResult> GeneratePaths([FromBody] JObject payload, [FromQuery] WebWalker? walker)
        {
            var result = await _mediator.Send(new GetWebPathQuery
            {
                UserId = await GetUserIdOrThrow(),
                Payload = payload,
                Type = WebPathType.Normal,
                WebWalker = walker
            });
            Request.Headers["X-Web-Walker"] = result.Walker.ToString();
            return Ok(result.Result);
        }
    }
}