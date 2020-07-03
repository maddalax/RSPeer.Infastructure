using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Api.Controllers.Base;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.Migration;
using RSPeer.Application.Features.ApiClients.Commands;
using RSPeer.Application.Features.ApiClients.Queries;
using RSPeer.Application.Features.UserManagement.Users.Queries.Options;

namespace RSPeer.Api.Controllers
{
    public class ApiClientController : BaseController
    {
        [HttpPost]
        [LoggedIn]
        [ReadOnly]
        public async Task<IActionResult> Create()
        {
            var result = await Mediator.Send(new CreateApiClientCommand
            {
                User = await GetUser(new UserLookupOptions {AllowCached = true})
            });
            return Ok(result);
        }
        
        [HttpGet]
        [LoggedIn]
        public async Task<IActionResult> Get()
        {
            var result = await Mediator.Send(new GetApiClientQuery
            {
                User = await GetUser(new UserLookupOptions {AllowCached = true})
            });
            return Ok(result);
        }
        
        [HttpPost]
        [LoggedIn]
        [ReadOnly]
        public async Task<IActionResult> Delete()
        {
            var result = await Mediator.Send(new DeleteApiClientCommand
            {
                User = await GetUser(new UserLookupOptions {AllowCached = true})
            });
            return Ok(result);
        }
    }
}