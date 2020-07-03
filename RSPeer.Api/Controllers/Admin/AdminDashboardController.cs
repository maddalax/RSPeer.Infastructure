using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.RateLimit;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.Admin.Dashboard.Commands;
using RSPeer.Application.Features.Admin.Dashboard.Queries;
using RSPeer.Domain.Constants;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers.Admin
{
    public class AdminDashboardController : BaseController
    {
        [HttpPost]
        [RateLimit(2, 1)]
        public async Task<IActionResult> GenerateKey()
        {
            var user = await GetUser();
            AssertOwner(user);
            await Mediator.Send(new GenerateDashboardKeyCommand
            {
                UserId = user.Id,
                Email = user.Email
            });
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> ValidateKey([FromQuery] string key)
        {
            var user = await GetUser();
            AssertOwner(user);
            try
            {
                await Mediator.Send(new ValidateDashboardKeyQuery
                {
                    UserId = user.Id,
                    Key = key
                });
                return Ok(true);
            }
            catch (AuthorizationException)
            {
                return Ok(false);
            }
        }

        private void AssertOwner(User user)
        {
            if (user.Groups.All(w => w.Id != GroupConstants.OwnersId))
            {
                throw new AuthenticationException();
            }
        }
    }
}