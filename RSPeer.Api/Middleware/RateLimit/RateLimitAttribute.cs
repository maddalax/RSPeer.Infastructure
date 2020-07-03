using Microsoft.AspNetCore.Mvc;

namespace RSPeer.Api.Middleware.RateLimit
{
	public class RateLimitAttribute : TypeFilterAttribute
	{
		public RateLimitAttribute(int maxRequests, int lengthInMinutes) : base(typeof(RateLimitFilter))
		{
			Arguments = new object[] { maxRequests, lengthInMinutes };
		}
	}
}