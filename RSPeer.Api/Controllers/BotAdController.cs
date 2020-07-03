using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Application.Features.BotAds.Models;
using RSPeer.Application.Features.BotAds.Queries;
using RSPeer.Application.Features.UserData.Commands;
using RSPeer.Application.Features.UserData.Queries;

namespace RSPeer.Api.Controllers
{
    [LoggedIn]
    public class BotAdController : BaseController
    {
        private readonly IMediator _mediator;

        public BotAdController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string tag)
        {
            var userId = await GetUserIdOrThrow();
            var ads = await _mediator.Send(new GetBotAdsQuery
            {
                UserId = userId,
                ClientTag = Guid.Parse(tag)
            });
            return Ok(ads);
        }

        [HttpPost]
        [Owner]
        public async Task<IActionResult> Save([FromBody] IEnumerable<BotAd> ads)
        {
            if (ads == null)
            {
                throw new Exception("Ads was null.");
            }
            await _mediator.Send(new SaveUserJsonDataCommand
            {
                Key = "internal_bot_ads",
                UserId = 1189,
                Value = ads
            });
            return NoContent();
        }
        
        [HttpGet]
        [Owner]
        public async Task<IActionResult> List()
        {
            var ads = await _mediator.Send(new GetUserJsonDataQuery
            {
                Key = "internal_bot_ads",
                UserId = 1189
            });
            return Ok(ads);
        }
    }
}