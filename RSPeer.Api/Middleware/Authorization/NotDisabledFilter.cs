using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RSPeer.Api.Utilities;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Application.Features.UserManagement.Users.Queries.Options;

namespace RSPeer.Api.Middleware.Authorization
{
	public class NotDisabledFilter : IAsyncAuthorizationFilter
	{
		private readonly IMediator _mediator;

		public NotDisabledFilter(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			context.HttpContext.Items.Remove("CurrentUser");
			
			var user = await _mediator.Send(new GetUserBySessionOrApiKeyQuery
			{
				Session = HttpUtilities.TryGetSession(context.HttpContext),
				Options = new UserLookupOptions
				{
					AllowCached = false,
					FullUser = true
				}
			});

			if (user == null)
			{
				context.Result = new UnauthorizedResult();
				return;
			}
			
			if (user.Disabled)
			{
				context.Result = new UnauthorizedObjectResult(new {error = "Your account has been disabled."});
				return;
			}
			
			context.HttpContext.Items["CurrentUser"] = user;
		}
	}
}