using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Features.UserData.Commands;
using RSPeer.Application.Features.UserData.Queries;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers
{
    public class BotPreferenceController : BaseController
    {
        private readonly IMediator _mediator;

        public BotPreferenceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [UserId]
        [RateLimit(500, 1)]
        public async Task<IActionResult> Get()
        {
            var userId = (await GetUserId()).GetValueOrDefault(-1);
            var pref = await _mediator.Send(new GetBotPreferencesQuery
            {
                UserId = userId
            });
            return Ok(pref);
        }

        [HttpPost]
        [UserId]
        [RateLimit(50, 1)]
        public async Task<IActionResult> Save([FromQuery] string key, string value)
        {
            var userId = (await GetUserId()).GetValueOrDefault(-1);
            var pref = await _mediator.Send(new GetBotPreferencesQuery
            {
                UserId = userId
            });
            var obj = JObject.FromObject(pref, JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

            if (!obj.ContainsKey(key))
            {
                return NoContent();
            }

            obj[key] = value;

            var converted = System.Text.Json.JsonSerializer.Deserialize<object>(obj.ToString());
            await _mediator.Send(new SaveUserJsonDataCommand
                {Key = "bot_preferences", UserId = userId, Value = converted});

            return Ok(obj.ToObject<BotPreferences>());
        }

        [HttpPost]
        [UserId]
        [RateLimit(50, 1)]
        public async Task<IActionResult> Overwrite([FromBody] BotPreferences preferences)
        {
            var userId = (await GetUserId()).GetValueOrDefault(-1);
            await _mediator.Send(new SaveUserJsonDataCommand
                {Key = "bot_preferences", UserId = userId, Value = preferences});
            return NoContent();
        }
    }
}