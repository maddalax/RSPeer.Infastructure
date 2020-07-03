using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using RSPeer.Application.Features.Discord.Events;
using RSPeer.Application.Features.Discord.Helpers;
using RSPeer.Application.Features.Discord.Models;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Common.Enviroment;

namespace RSPeer.Application.Features.Discord.Commands
{
	public class RegisterDiscordBotCommandHandler : IRequestHandler<RegisterDiscordBotCommand, Unit>
	{
		private readonly IDiscordSocketClientProvider _provider;
		private readonly IServiceScopeFactory _factory;
		private DiscordSocketClient _client;
		private readonly DiscordRoleHelper _roleHelper;
		private readonly ILogger<RegisterDiscordBotCommandHandler> _logger;
		private readonly IDistributedLockFactory _lockFactory;
		private readonly IRedisService _redis;

		public RegisterDiscordBotCommandHandler(IDiscordSocketClientProvider provider, IServiceScopeFactory factory, IConfiguration configuration, ILogger<RegisterDiscordBotCommandHandler> logger, IRedisService redis)
		{
			_provider = provider;
			_factory = factory;
			_roleHelper = new DiscordRoleHelper(configuration);
			_logger = logger;
			_redis = redis;
			_lockFactory = RedLockFactory.Create(new List<RedLockMultiplexer>
			{
				_redis.Connection()
			});
		}

		public async Task<Unit> Handle(RegisterDiscordBotCommand request, CancellationToken cancellationToken)
		{
			using (var botLock = await _lockFactory.CreateLockAsync("rspeer_discord_bot_lock", TimeSpan.FromMinutes(30)))
			{
				if (!botLock.IsAcquired)
				{
					return Unit.Value;
				}	
				_client = await _provider.Get();
				_client.MessageReceived += OnMessage;
				_client.Disconnected += OnDisconnect;
			}
			return Unit.Value;
		}

		private async Task OnDisconnect(Exception e)
		{
			if (_client != null && _client.ConnectionState == ConnectionState.Disconnected)
			{
				_client = await _provider.Get();
				await _client.StartAsync();
				_client.MessageReceived += OnMessage;
				_client.Disconnected += OnDisconnect;
			}
		}
		
		private async Task OnMessage(SocketMessage message)
		{
			if (!message.Content.StartsWith("!"))
			{
				return;
			}

			using var scope = _factory.CreateScope();
			try
			{
				var payload = new DiscordMessageEvent
				{
					Id = message.Id,
					Content = message.Content,
					IsDirector = _roleHelper.HasRole(DiscordRole.Director, message),
					IsTokenSeller = _roleHelper.HasRole(DiscordRole.TokenSeller, message),
					IsBot = message.Author.IsBot,
					Username = message.Author.Username,
					Discriminator = message.Author.Discriminator,
					UserId = message.Author.Id,
					InGuild = _roleHelper.InGuild(message),
					Channel = message.Channel.Name,
					ChannelId = message.Channel.Id,
					GuildId = _roleHelper.GetGuildId(message),
					Processor = EnviromentExtensions.Idenitifer
				};
				var mediator = scope.ServiceProvider.GetService<IMediator>();
				await mediator.Publish(payload);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to process discord message.");
			}
		}
	}
}