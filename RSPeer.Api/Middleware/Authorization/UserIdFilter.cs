using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RSPeer.Api.Utilities;
using RSPeer.Application.Features.UserManagement.Users.Queries;

namespace RSPeer.Api.Middleware.Authorization
{
    public class UserIdFilter : IAsyncAuthorizationFilter
    {
        private readonly IMediator _mediator;

        public UserIdFilter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            context.HttpContext.Items.Remove("CurrentUserId");

            var user = await _mediator.Send(new GetUserIdBySessionQuery
            {
                Session = HttpUtilities.TryGetSession(context.HttpContext)
            });
			
            if (user == null)
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                context.HttpContext.Items["CurrentUserId"] = user;
            }
        }
    }
}