using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Entities;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RSPeer.Application.Features.Instance.Commands
{
	public class ParseClientInfoCommandHandler : IRequestHandler<ParseClientInfoCommand, RunescapeClient>
	{
		private readonly ILogger<ParseClientInfoCommandHandler> _client;
		private readonly IConfiguration _configuration;

		public ParseClientInfoCommandHandler(ILogger<ParseClientInfoCommandHandler> client, IConfiguration configuration)
		{
			_client = client;
			_configuration = configuration;
		}

		public Task<RunescapeClient> Handle(ParseClientInfoCommand request, CancellationToken cancellationToken)
		{
			try
			{
				ClientInfo info;
				try
				{
					info = request.IsXor
						? XorAndDeserialize(request)
						: JsonSerializer.Deserialize<ClientInfo>(request.RawClientInfo);
				}
				catch (Exception)
				{
					info = XorAndDeserialize(request);
				}

				if (info == null)
				{
					_client.LogWarning("Received client updated with no data.", request.RawClientInfo);
					return null;
				}

				var client = new RunescapeClient
				{
					IsBanned = info.IsBanned,
					Ip = info.IpAddress,
					ProxyIp = info.ProxyIp,
					LastUpdate = DateTimeOffset.UtcNow,
					MachineName = info.MachineName,
					RunescapeEmail = info.RunescapeLogin,
					ScriptName = info.ScriptName,
					UserId = request.UserId,
					OperatingSystem = info.OperatingSystem,
					Tag = request.Identifier,
					AtInstanceLimit = info.AtInstanceLimit,
					IsRepoScript = info.IsRepoScript,
					ScriptId = info.ScriptId.ToString(),
					ScriptClassName = info.ScriptClassName,
					ScriptDeveloper = info.ScriptDeveloper,
					JavaVersion = info.JavaVersion,
					MachineUserName = info.MachineUserName,
					Version = info.Version,
					Rsn = info.Rsn
				};

				try
				{
					client.RspeerFolderFileNames = JsonSerializer.Serialize(info.RspeerFolderFileNames);
				}
				catch (Exception)
				{
					// ignored
				}

				var enhanced = EnhanceClient(client);
				return Task.FromResult(enhanced);
			}
			catch (Exception e)
			{
				_client.LogError(e, "Failed to parse client information.");
				return null;
			}
		}

		private ClientInfo XorAndDeserialize(ParseClientInfoCommand command)
		{
			var bytes = Convert.FromBase64String(command.RawClientInfo);
			var decrypted = StringExtensions.Xor(bytes, _configuration.GetValue<string>(command.Game == Game.Osrs ? "Encryption:Xor" : "Encryption:XorInuvation"));
			var data = Encoding.UTF8.GetString(decrypted);
			return JsonSerializer.Deserialize<ClientInfo>(data, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});
		}

		private RunescapeClient EnhanceClient(RunescapeClient client)
		{
			client.RunescapeEmail = client.RunescapeEmail?.Trim().ToLower();
			client.Rsn = client.Rsn?.Trim().ToLower();
			client.Ip = client.Ip?.Trim().ToLower();
			client.ProxyIp = client.ProxyIp?.Trim().ToLower();
			client.ScriptName = client.ScriptName?.Trim();
			client.MachineName = client.MachineName?.Trim().ToLower();
			client.Id = 0;
			client.LastUpdate = DateTimeOffset.Now;
			var key = $"{client.Tag}:{client.RunescapeEmail}:{client.ScriptName}:{client.Ip}:{client.ProxyIp}:{client.OperatingSystem}:{client.IsBanned}"
				.ToLower().Trim();
			client.Hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
			return client;
		}
	}
}