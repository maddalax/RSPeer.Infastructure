using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using RSPeer.Application.Features.Bot.Models;
using RSPeer.Application.Features.Files.Queries;
using RSPeer.Common.Extensions;

namespace RSPeer.Application.Features.Bot.Commands
{
	public class ObfuscateBotCommandHandler : IRequestHandler<ObfuscateBotCommand, byte[]>
	{		
		private readonly IMediator _mediator;
		private readonly HttpClient _client;

		public ObfuscateBotCommandHandler(IMediator mediator, IHttpClientFactory factory, IConfiguration configuration)
		{
			_mediator = mediator;
			_client = factory.CreateClient("Compiler");
			_client.DefaultRequestHeaders.Add("Authorization", configuration.GetValue<string>("Compiler:Token"));
			_client.BaseAddress = new Uri(configuration.GetValue<string>("Compiler:Path"));
		}
		
		public async Task<byte[]> Handle(ObfuscateBotCommand request, CancellationToken cancellationToken)
		{
			var config = await _mediator.Send(new GetFileQuery { Name = "bot-obfuscate-config.xml" }, cancellationToken);

			if (string.IsNullOrEmpty(request.Path))
			{
				return new byte[0];
			}
			
			var bytes = File.ReadAllBytes(request.Path);
			var obfuscate = new ObfuscateRequest
			{
				Bytes = bytes,
				Config = Encoding.Default.GetString(config)
			};
			
			var result = await _client.PostAsync("/api/obfuscate/execute",
				new StringContent(JsonSerializer.Serialize(obfuscate), Encoding.UTF8, "application/json"),
				cancellationToken);
			
			var stream = await result.Content.ReadAsStreamAsync();

			return stream.ToByteArray();

		}
	}
}