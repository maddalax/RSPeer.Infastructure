using Microsoft.AspNetCore.Mvc;
using RSPeer.Domain.Constants;

namespace RSPeer.Api.Middleware.Authorization
{
	public class OwnerAttribute : TypeFilterAttribute
	{
		public OwnerAttribute() : base(typeof(GroupAuthorizationFilter))
		{
			Arguments = new object[] { GroupConstants.OwnersId };
		}
	}
	
	public class LoggedInAttribute : TypeFilterAttribute
	{
		public LoggedInAttribute() : base(typeof(LoggedInFilter))
		{
		}
	}
	
	public class UserIdAttribute : TypeFilterAttribute
	{
		public UserIdAttribute() : base(typeof(UserIdFilter))
		{
		}
	}
	
	public class CompilerAttribute : TypeFilterAttribute
	{
		public CompilerAttribute() : base(typeof(CompilerFilter))
		{
		}
	}

	public class NotDisabledAttribute : TypeFilterAttribute
	{
		public NotDisabledAttribute() : base(typeof(NotDisabledFilter))
		{
		}
	}
}