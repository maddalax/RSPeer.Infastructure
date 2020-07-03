using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RSPeer.Api.Activators;
using RSPeer.Api.Jobs.Children;
using RSPeer.Api.Jobs.Commands;
using RSPeer.Api.Mappers;
using RSPeer.Api.Middleware.Authorization;
using RSPeer.Api.Middleware.Exceptions;
using RSPeer.Application.Features.BotPanel;
using RSPeer.Application.Features.Discord.Commands;
using RSPeer.Application.Features.Discord.Helpers;
using RSPeer.Application.Features.Discord.Models;
using RSPeer.Application.Features.Discord.Setup;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Application.Features.Scripts.Commands.CreateScript;
using RSPeer.Application.Features.WebWalker.Acuity;
using RSPeer.Application.Features.WebWalker.DaxWalker;
using RSPeer.Application.Infrastructure.AutoMapper;
using RSPeer.Application.Infrastructure.Caching;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Application.Infrastructure.Caching.Listeners;
using RSPeer.Application.Infrastructure.Models;
using RSPeer.ForumsMigration;
using RSPeer.Infrastructure.Cognito.Users.Commands;
using RSPeer.Infrastructure.File;
using RSPeer.Infrastructure.File.Base;
using RSPeer.Infrastructure.Gitlab;
using RSPeer.Infrastructure.Gitlab.Base;
using RSPeer.Persistence;
using SendGrid;
using Sentry.AspNetCore;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace RSPeer.Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.AddSentry();
				loggingBuilder.AddApplicationInsights(Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey"));
			});

			services.AddAutoMapper(typeof(AutoMapperProfile).GetTypeInfo().Assembly);

			services.AddMediatR(typeof(CreateScriptCommandHandler).GetTypeInfo().Assembly,
				typeof(CognitoSignUpCommand).GetTypeInfo().Assembly,
				typeof(RegisterJobsCommand).GetTypeInfo().Assembly);

			services.AddHttpClient();

			services.AddEntityFrameworkNpgsql();

			services.AddDbContextPool<RsPeerContext>((provider, builder) =>
			{
				builder.UseNpgsql(Configuration.GetConnectionString("Postgres"),
					options => { options.MigrationsAssembly(typeof(Startup).Assembly.FullName); });
			});

			services.AddDbContext<DiscourseContext>((provider, builder) =>
			{
				builder.UseNpgsql(Configuration.GetConnectionString("Discourse"));
			});

			services.AddSingleton<IGitlabService, GitLabService>();
			services.AddSingleton<IRedisPubSubListener, RedisPubSubCommandHandler>();
			services.AddScoped<IBotLauncherService, BotLauncherService>();
			services.AddSingleton<IRedisService, RedisService>();
			services.AddSingleton<IDashboardAuthorizationFilter, HangfireDashboardAuthorizationFilter>();
			services.AddScoped<DaxWalkerService>();
			services.AddScoped<AcuityWalkerService>();

			services.AddMemoryCache();

			services.AddCors(o => o.AddPolicy("AllowCors", builder =>
			{
				builder
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowAnyOrigin();
			}));

			// Jobs
			services.AddScoped<InstanceCloseJob>();
			services.AddScoped<SetClientCountJob>();
			services.AddScoped<SaveIpAccess>();
			services.AddScoped<SetTrueScriptCounts>();
			services.AddScoped<CheckInuvationAccess>();
			services.AddScoped<SendDiscordAlerts>();
			services.AddScoped<ClearMessagesJob>();
			services.AddScoped<ArchiveRunescapeClients>();

			services.AddSingleton<IFileStorage, SpacesService>();

			services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.ForwardedHeaders =
					ForwardedHeaders.All;
				var cloudflare = Configuration.GetValue<string>("Cloudflare:IpList").Split(",").Select(w => w.Split("/")[0]).Select(IPAddress.Parse).ToList();
				cloudflare.ForEach(ip => options.KnownProxies.Add(ip));
			});

			services.AddControllers()
				.AddNewtonsoftJson(options =>
				{
					options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
				})
				.AddJsonOptions(w =>
				{
					w.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
				});

			services.AddHangfire(config =>
			{
				config.UsePostgreSqlStorage(Configuration.GetConnectionString("Postgres"), new PostgreSqlStorageOptions
				{
					PrepareSchemaIfNecessary = false
				});
			});

			services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });


			services.AddSingleton<IDiscordSocketClientProvider, DiscordSocketClientProvider>();
			services.AddSingleton<DiscordRoleHelper>();

			services.AddSingleton<IUserFactory, SentryUserFactory>();

			services.AddScoped<ISendGridClient>(a =>
			{
				var config = a.GetService<IConfiguration>();
				return new SendGridClient(config.GetValue<string>("SendGrid:ApiKey"));
			});
			
			EntityMapperExtensions.AddEntityMappers();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider provider)
		{
			app.UseForwardedHeaders();
			app.UseHsts();
			GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(app.ApplicationServices));

			app.UseMiddleware<ExceptionMiddleware>();
			app.UseHttpsRedirection();
			app.UseCors("AllowCors");

			app.UseHangfireServer();

			app.UseHangfireDashboard(Configuration.GetValue<string>("Hangfire:Path"),
				new DashboardOptions
				{
					Authorization = new[]
					{
						provider.GetService<IDashboardAuthorizationFilter>()
					}
				});

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			RegisterAsyncServices(provider.GetService<IServiceScopeFactory>());
		}

		private async Task RegisterAsyncServices(IServiceScopeFactory factory)
		{
			using var scope = factory.CreateScope();
			var logger = scope.ServiceProvider.GetService<ILogger<Startup>>();
			logger.LogTrace("Registered services.");
			var provider = scope.ServiceProvider;
			var mediator = provider.GetService<IMediator>();
			try
			{
				await mediator.Send(new RegisterJobsCommand());
				await mediator.Send(new RegisterDiscordBotCommand());
			}
			catch (Exception e)
			{
				logger.LogCritical(e, "Failed to execute startup services.");
				await mediator.Send(new SendDiscordWebHookCommand
				{
					Critical = true,
					Message = $"Failed to register startup services for {Dns.GetHostName()}. Reason: {e}",
					Type = DiscordWebHookType.Log
				});
			}
		}
	}
}