using System.Linq;
using Hangfire.Dashboard;
using Microsoft.Extensions.Configuration;

namespace RSPeer.Api.Middleware.Authorization
{
	public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
	{
		private readonly IConfiguration _configuration;

		public HangfireDashboardAuthorizationFilter(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public bool Authorize(DashboardContext context)
		{
			var http = context.GetHttpContext();
			var token = _configuration.GetValue<string>("Hangfire:DashboardToken");

			var cookie = http.Request.Cookies.FirstOrDefault(w => w.Key == "hangfire:token");

			if (!string.IsNullOrEmpty(cookie.Value))
			{
				return cookie.Value == token;
			}
			
			var key = http.Request.Query["token"].FirstOrDefault();
			
			if (key == null)
			{
				return false;
			}

			if (key != token)
			{
				return false;
			}
			
			http.Response.Cookies.Append("hangfire:token", token);

			return true;
		}
	}
}