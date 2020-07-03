using System;

namespace RSPeer.Common.Enviroment
{
	public static class EnviromentExtensions
	{
		public static string Idenitifer { get; } = Guid.NewGuid().ToString();
		
		public static bool IsDevelopmentMode()
		{
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			return env == "Development";
		}
	}
}