using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RSPeer.Api.Utilities;
using RSPeer.Application.Features.Admin.Dashboard.Queries;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Application.Features.UserManagement.Users.Queries.Options;
using RSPeer.Common.Enviroment;
using RSPeer.Domain.Constants;

namespace RSPeer.Api.Middleware.Authorization
{
	public class GroupAuthorizationFilter : IAsyncAuthorizationFilter
	{
		private readonly IMediator _mediator;

		public GroupAuthorizationFilter(int groupId, IMediator mediator)
		{
			_mediator = mediator;
			GroupId = groupId;
		}

		private int GroupId { get; }

		public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			context.HttpContext.Items.Remove("CurrentUser");
			
			var session = HttpUtilities.TryGetSession(context.HttpContext);
			
			if (string.IsNullOrEmpty(session))
			{
				context.Result = new UnauthorizedResult();
				return;
			}

			var user = await _mediator.Send(new GetUserBySessionOrApiKeyQuery
				{ Session = session, Options = new UserLookupOptions { IncludeGroups = true } });

			if (user == null)
			{
				context.Result = new UnauthorizedResult();
				return;
			}
			
			var hasGroup = user.Groups.Any(w => w.Id == GroupId);
			
			if (!hasGroup)
			{
				context.Result = new UnauthorizedResult();
			}

			/*
			if (GroupId == GroupConstants.OwnersId && !EnviromentExtensions.IsDevelopmentMode())
			{
				var dashboardKey = HttpUtilities.GetDashboardKey(context.HttpContext);
				await _mediator.Send(new ValidateDashboardKeyQuery {Key = dashboardKey, UserId = user.Id });
			}
			*/
			
			context.HttpContext.Items["CurrentUser"] = user;
			context.HttpContext.Items["CurrentUserId"] = user.Id;
		}
	}
}