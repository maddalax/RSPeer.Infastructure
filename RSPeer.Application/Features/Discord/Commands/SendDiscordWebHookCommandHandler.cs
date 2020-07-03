using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using RSPeer.Application.Features.Discord.Models;
using RSPeer.Common.Enviroment;

namespace RSPeer.Application.Features.Discord.Commands
{
	public class SendDiscordWebHookCommandHandler : IRequestHandler<SendDiscordWebHookCommand, Unit>
	{
		private readonly HttpClient _client;
		private readonly IConfiguration _configuration;

		public SendDiscordWebHookCommandHandler(IHttpClientFactory factory, IConfiguration configuration)
		{
			_client = factory.CreateClient("Discord");
			_configuration = configuration;
		}

		public async Task<Unit> Handle(SendDiscordWebHookCommand request, CancellationToken cancellationToken)
		{
			if (EnviromentExtensions.IsDevelopmentMode())
			{
				Console.WriteLine(request.Type + " | " + request.Message);
				//return Unit.Value;
			}
			
			if (request.Critical)
			{
				var director = _configuration.GetValue<string>("Discord:DirectorId");
				request.Message = $"{director} [CRITICAL] - {request.Message}";
			}
			
			var dict = new Dictionary<string, object> { { "content", request.Message } };
			var json = JsonSerializer.Serialize(dict);
			await _client.PostAsync(GetWebHook(request.Type), new StringContent(json,
				Encoding.UTF8, "application/json"), cancellationToken);
			return Unit.Value;
		}

		private string GetWebHook(DiscordWebHookType type)
		{
			return type switch
			{
				DiscordWebHookType.ScriptUpdate => _configuration.GetValue<string>("Discord:ScriptUpdateHook"),
				DiscordWebHookType.Log => _configuration.GetValue<string>("Discord:AdminLog"),
				DiscordWebHookType.InuvationPurchase => _configuration.GetValue<string>("Discord:InuvationPurchase"),
				DiscordWebHookType.DiscordBotLog => _configuration.GetValue<string>("Discord:DiscordBotLog"),
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
		}
	}
}