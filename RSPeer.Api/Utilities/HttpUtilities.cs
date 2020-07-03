using System.Linq;
using Microsoft.AspNetCore.Http;

namespace RSPeer.Api.Utilities
{
	public static class HttpUtilities
	{
		public static string GetIp(HttpContext context)
		{
			var forwarded = context.Request.Headers["X-Client-IP"].FirstOrDefault();
			var cloudflare = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
			return forwarded ?? cloudflare ?? context.Connection.RemoteIpAddress
				       .ToString();
		}

		public static string GetDashboardKey(HttpContext context)
		{
			var headers = context.Request.Headers;
			
			return !headers.ContainsKey("X-Dashboard-Key") ? null : headers["X-Dashboard-Key"].FirstOrDefault();
		}

		public static string TryGetSession(HttpContext context)
		{
			var headers = context.Request.Headers;

			if (headers.ContainsKey("ApiClient"))
			{
				var header = context.Request.Headers["ApiClient"].FirstOrDefault()?.Trim();
				return string.IsNullOrWhiteSpace(header) ? null : header;
			}
			
			return TryGetSessionInternal(context);
		}
		
		private static string TryGetSessionInternal(HttpContext context)
		{
			var headers = context.Request.Headers;
			
			if (!headers.ContainsKey("Authorization"))
				return null;
			
			string header = context.Request.Headers["Authorization"];
			
			if (string.IsNullOrEmpty(header))
				return null;
			
			header = header.Trim();

			if (!header.StartsWith("Bearer ") && !header.StartsWith("bearer "))
			{
				return null;
			}
			
			var token = header.StartsWith("Bearer")
				? header.Substring("Bearer ".Length)
				: header.Substring("bearer ".Length);

			token = token.Trim();
						
			return token == "null" ? null : string.IsNullOrWhiteSpace(token) ? null : token;
		}
	}
}