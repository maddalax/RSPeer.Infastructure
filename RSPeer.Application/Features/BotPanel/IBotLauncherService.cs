using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RSPeer.Domain.Dtos;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.BotPanel
{
	public interface IBotLauncherService
	{
		Task<object> GetConnectedLaunchers(int userId);
		
		Task<object> SendJson(JObject body, int userId);

		Task Send(string message, int userId, string tag = null);

		Task<object> Kill(string socket, int userId);

		Task<IEnumerable<RunescapeClientDto>> ConnectedClients(int userId);

		Task<object> GetLogs(int userId, string socketId, string top, string skip, string type);
		
		Task<Guid> GetKey(int userId);

		Task<Guid> ChangeKey(int userId);

		Task<IEnumerable<QuickLaunch>> GetQuickLaunch(int userId);

		Task SaveQuickLaunch(int userId, QuickLaunch launch);

		Task DeleteQuickLaunch(int userId, string id);

		Task DeleteProxy(int userId, string id);

		Task SaveProxy(int userId, RsClientProxy proxy);

		Task<IEnumerable<RsClientProxy>> GetProxies(int userId);
	}
}