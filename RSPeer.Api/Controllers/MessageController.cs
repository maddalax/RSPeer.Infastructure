using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Features.Messaging.Commands;
using RSPeer.Application.Features.Messaging.Queries;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers
{
    [LoggedIn]
    public class MessageController : BaseController
    {
        [HttpPost]
        [RateLimit(1000, 1)]
        public async Task<ActionResult<IEnumerable<RunescapeClient>>> Send([FromBody] SendRemoteMessageCommand command)
        {
            var user = await GetUser();
            command.UserId = user.Id;
            return Ok(await Mediator.Send(command));
        }
        
        [HttpGet]
        [RateLimit(1000, 1)]
        public async Task<ActionResult<IEnumerable<RunescapeClient>>> Get([FromQuery] string consumer)
        {
            var user = await GetUser();
            var command = new GetRemoteMessagesQuery {UserId = user.Id, Consumer = consumer};
            return Ok(await Mediator.Send(command));
        }
        
        [HttpPost]
        [RateLimit(1000, 1)]
        public async Task<ActionResult<IEnumerable<RunescapeClient>>> Consume([FromQuery] int message)
        {
            var user = await GetUser();
            var command = new ConsumeRemoteMessageCommand {UserId = user.Id, MessageId = message};
            return Ok(await Mediator.Send(command));
        }
    }
}