using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RSPeer.Api.Utilities;
using RSPeer.Application.Features.UserManagement.Users.Queries;

namespace RSPeer.Api.Middleware.Authorization
{
	public class LoggedInFilter : IAsyncAuthorizationFilter
	{
		private readonly IMediator _mediator;

		public LoggedInFilter(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			context.HttpContext.Items.Remove("CurrentUser");

			var user = await _mediator.Send(new GetUserBySessionOrApiKeyQuery
			{
				Session = HttpUtilities.TryGetSession(context.HttpContext)
			});
			
			if (user == null)
			{
				context.Result = new UnauthorizedResult();
			}
			else
			{
				context.HttpContext.Items["CurrentUser"] = user;
			}
		}
	}
}