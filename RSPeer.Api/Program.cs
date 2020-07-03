using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using RSPeer.Common.Enviroment;

namespace RSPeer.Api
{
	public class Program
	{
		public static readonly double Version = 3.01;
		
		public static void Main(string[] args)
		{
			var host = CreateWebHostBuilder(args).Build();
			host.Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			var builder = WebHost.CreateDefaultBuilder(args)
				.ConfigureKestrel(w => w.AllowSynchronousIO = true)
				.ConfigureLogging((c, l) =>
				{
					l.AddConfiguration(c.Configuration);
					l.AddSentry();
				})
				.UseSentry(s => s.Release = $"rspeer-api-{Version}")
				.UseStartup<Startup>();
			
			if (EnviromentExtensions.IsDevelopmentMode())
			{
				builder.UseUrls("http://*:80");
			}
			return builder;
		}
	}
}





