using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RSPeer.Application.Features.Migration.Queries;

namespace RSPeer.Api.Middleware.Migration
{
	public class ReadOnlyFilter : IAsyncAuthorizationFilter
	{
		private readonly IMediator _mediator;

		public ReadOnlyFilter(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			var enabled = await _mediator.Send(new IsReadOnlyModeQuery());
			
			if (!enabled)
			{
				return;
			}
			
			context.HttpContext.Response.StatusCode = (int) HttpStatusCode.ServiceUnavailable;
			context.Result = new ObjectResult(new {error = "This feature is down for maintenance, services will be restored shortly. Please check status on https://rspeer.org."});
		}
	}
}