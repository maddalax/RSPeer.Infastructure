using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using RSPeer.Application.Features.Files.Queries;
using RSPeer.Application.Features.Scripts.Commands.CompileScript.Models;

namespace RSPeer.Application.Features.Scripts.Commands.CompileScript
{
	public class CompileScriptHandler : IRequestHandler<CompileScriptCommand, CompiledScript>
	{
		private readonly IMediator _mediator;
		private readonly HttpClient _client;

		public CompileScriptHandler(IMediator mediator, IHttpClientFactory factory, IConfiguration configuration)
		{
			_mediator = mediator;
			_client = factory.CreateClient("Compiler");
			_client.DefaultRequestHeaders.Add("Authorization", configuration.GetValue<string>("Compiler:Token"));
			_client.BaseAddress = new Uri(configuration.GetValue<string>("Compiler:Path"));
		}
		
		public async Task<CompiledScript> Handle(CompileScriptCommand request, CancellationToken cancellationToken)
		{
			var config = await _mediator.Send(new GetFileQuery { Name = "script-obfuscate-config.xml" }, cancellationToken);
			var compileRequest = new CompileRequest
			{
				GitPath = request.GitlabUrl,
				ObfuscateConfig = Encoding.Default.GetString(config),
				Game = request.Game
			};
			
			var res = await _client.PostAsync("/api/git/buildJar", new StringContent(
				JsonSerializer.Serialize(compileRequest), Encoding.Default, "application/json"), 
				cancellationToken);

			if (res.StatusCode != HttpStatusCode.OK)
			{
				var error = await res.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<CompileResult>(error);
				throw new CompileException(result);
			}
			
			var bytes = await res.Content.ReadAsByteArrayAsync();

			if (bytes.Length == 0)
			{
				throw new Exception("Failed to compile script by url: " + request.GitlabUrl);
			}
			
			return new CompiledScript
			{
				Content = bytes
			};
		}
	}
}