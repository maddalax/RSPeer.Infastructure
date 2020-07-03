using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RSPeer.Api.Utilities;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Application.Features.UserManagement.Users.Queries.Options;
using RSPeer.Domain.Entities;

namespace RSPeer.Api.Controllers.Base
{
	[ApiController]
	[Route("api/[controller]/[action]")]
	public abstract class BaseController : Controller
	{
		private IMediator _mediator;

		protected IMediator Mediator => _mediator ?? (_mediator = HttpContext.RequestServices.GetService<IMediator>());
		
		public async Task<User> GetUser(UserLookupOptions options = null)
		{
			if (HttpContext.Items.ContainsKey("CurrentUser")) return (User) HttpContext.Items["CurrentUser"];
			var token = HttpUtilities.TryGetSession(HttpContext);
			return token == null ? null : await Mediator.Send(new GetUserBySessionOrApiKeyQuery { Session = token, Options = options});
		}
		
		public async Task<int?> GetUserId()
		{
			if (HttpContext.Items.ContainsKey("CurrentUserId")) return (int) HttpContext.Items["CurrentUserId"];
			var token = HttpUtilities.TryGetSession(HttpContext);
			return token == null ? null : await Mediator.Send(new GetUserIdBySessionQuery { Session = token });
		}
		
		public async Task<int> GetUserIdOrThrow()
		{
			var userId = await GetUserId();
			
			if (!userId.HasValue)
			{
				throw new AuthorizationException("You must be logged in.");
			}

			return userId.Value;
		}
	}
}