using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Nelibur.ObjectMapper;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RSPeer.Application.Features.Instance.Queries;
using RSPeer.Application.Features.Launchers.Queries;
using RSPeer.Application.Features.Messaging.Commands;
using RSPeer.Application.Features.UserData.Commands;
using RSPeer.Application.Features.UserData.Queries;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Application.Infrastructure.Caching.Commands;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Dtos;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RSPeer.Application.Features.BotPanel
{
    public class BotLauncherService : IBotLauncherService
    {
        private readonly string _socketManagerUrl;
        private readonly string _socketManagerKey;
        private readonly RsPeerContext _db;
        private readonly IMediator _mediator;
        private readonly HttpClient _client;

        private const string SaveQuickLaunchKey = "bot_launcher_quick_launch";
        private const string SaveProxyKey = "bot_launcher_proxies";

        public BotLauncherService(IConfiguration configuration, RsPeerContext db, IHttpClientFactory factory,
            IMediator mediator)
        {
            _socketManagerUrl = configuration.GetValue<string>("SocketManager:Url");
            _socketManagerKey = configuration.GetValue<string>("SocketManager:Key");
            _db = db;
            _client = factory.CreateClient("SocketManager");
            _mediator = mediator;
        }

        public async Task<object> GetConnectedLaunchers(int userId)
        {
            var key = await GetKey(userId);
            var data = await _client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_socketManagerUrl}/launcher/connected?key={_socketManagerKey}&userId={key}")
            });
            var content = await data.Content.ReadAsStringAsync();
            JObject current;
            try
            {
                current = JsonConvert.DeserializeObject<JObject>(content);
            }
            catch (Exception)
            {
                current = new JObject();
            }

            var registered = await _mediator.Send(new GetRegisteredLaunchersQuery {UserId = userId});
            foreach (var launcher in registered)
            {
                current[launcher.Tag.ToString()] = new JObject
                {
                    {"ip", launcher.Ip},
                    {"host", launcher.Host},
                    {"platform", launcher.Platform},
                    {"type", launcher.Platform},
                    {"userInfo", new JObject
                    {
                        {"username", launcher.MachineUsername}
                    }}
                };   
            }

            return current;
        }

        public async Task<object> SendJson(JObject body, int userId)
        {
            var socket = body.ContainsKey("socket") ? body.Value<string>("socket") : null;
            // Is new type of launcher
            if (socket != null && Guid.TryParse(socket, out _))
            {
                var payload = body.ContainsKey("payload") ? body.Value<JObject>("payload").ToString() : null;
                if (payload != null)
                {
                    await _mediator.Send(new SendRemoteMessageCommand
                    {
                        Source = "bot_panel_user_request",
                        Consumer = socket,
                        Message = payload,
                        UserId = userId
                    });
                    return new object();
                }
            }
            // TODO remove this legacy code !
            var key = await GetKey(userId);
            var data = await _client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"),
                RequestUri = new Uri($"{_socketManagerUrl}/launcher/send?key={_socketManagerKey}&userId={key}")
            });
            return JsonConvert.DeserializeObject<object>(await data.Content.ReadAsStringAsync());
        }

        public async Task Send(string message, int userId, string tag = null)
        {
            if (tag != null)
            {
                await _mediator.Send(new SendRemoteMessageCommand
                {
                    Source = "bot_panel_user_request",
                    Consumer = tag,
                    Message = message,
                    UserId = userId
                });
            }
        }

        public async Task<object> Kill(string socket, int userId)
        {
            var key = await GetKey(userId);
            var data = await _client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri =
                    new Uri(
                        $"{_socketManagerUrl}/launcher/clients/kill?key={_socketManagerKey}&id={socket}&userId={key}")
            });
            return JsonSerializer.Deserialize<object>(await data.Content.ReadAsStringAsync());
        }

        public async Task<IEnumerable<RunescapeClientDto>> ConnectedClients(int userId)
        {
            var clients = await _mediator.Send(new GetRunningClientsQuery {UserId = userId, OrderByDate = true});
            var grouped = clients.GroupBy(w => w.Tag).Select(w => w.First());
            return grouped.Select(TinyMapper.Map<RunescapeClientDto>);
        }

        public async Task<object> GetLogs(int userId, string socketId, string top, string skip, string type)
        {
            return new JObject();
        }

        public async Task<Guid> GetKey(int userId)
        {
            var full = await _mediator.Send(new GetUserByIdQuery {Id = userId, AllowCached = false});

            if (full == null)
            {
                return default;
            }

            if (full.LinkKey != null)
            {
                return full.LinkKey.Value;
            }

            full.LinkKey = Guid.NewGuid();
            _db.Users.Update(full);
            await _db.SaveChangesAsync();
            await _mediator.Send(new UpdateUserCacheCommand(full.Username));
            return full.LinkKey.Value;
        }

        public async Task<Guid> ChangeKey(int userId)
        {
            var full = await _mediator.Send(new GetUserByIdQuery {Id = userId, AllowCached = false});
            full.LinkKey = Guid.NewGuid();
            _db.Users.Update(full);
            await _db.SaveChangesAsync();
            await _mediator.Send(new UpdateUserCacheCommand(full.Username));
            return full.LinkKey.Value;
        }

        public async Task<IEnumerable<QuickLaunch>> GetQuickLaunch(int userId)
        {
            var result = await _mediator.Send(new GetUserJsonDataQuery
            {
                UserId = userId,
                Key = SaveQuickLaunchKey
            });
            return result.To<IEnumerable<QuickLaunch>>()?.OrderBy(w => w.Name).ToList() ?? new List<QuickLaunch>();
        }

        public async Task SaveQuickLaunch(int userId, QuickLaunch launch)
        {
            var current = (await GetQuickLaunch(userId)).ToList();
            var exists = current.FirstOrDefault(w =>
            {
                if (w.QuickLaunchId != null && launch.QuickLaunchId != null && w.QuickLaunchId == launch.QuickLaunchId)
                {
                    return true;
                }

                return w.Name == launch.Name;
            });

            if (exists == null)
            {
                launch.QuickLaunchId = Guid.NewGuid().ToString();
            }

            if (launch.Clients.Count > 1000)
                throw new Exception("Quick launch config must contain less than 1000 clients.");


            if (exists == null)
            {
                current.Add(launch);
            }
            else
            {
                current.Remove(exists);
                current.Add(launch);
            }

            if (current.Count >= 500)
                throw new Exception("You may not create more than 500 quick launch configurations.");

            await _mediator.Send(new SaveUserJsonDataCommand
                {Key = SaveQuickLaunchKey, Value = current, UserId = userId});
        }

        public async Task DeleteQuickLaunch(int userId, string id)
        {
            var current = (await GetQuickLaunch(userId)).ToList();
            var exists = current.FirstOrDefault(w =>
            {
                if (id == null)
                {
                    return false;
                }
                
                if (w.QuickLaunchId != null && w.QuickLaunchId == id)
                {
                    return true;
                }

                return w.Name == id;
            });
            
            if (exists != null)
            {
                current.Remove(exists);
            }

            await _mediator.Send(new SaveUserJsonDataCommand
                {Key = SaveQuickLaunchKey, Value = current, UserId = userId});
        }

        public async Task DeleteProxy(int userId, string id)
        {
            var current = (await GetProxies(userId)).ToList();
        
            var exists = current.FirstOrDefault(w =>
            {
                if (id == null)
                {
                    return false;
                }
                
                if (w.ProxyId != null && w.ProxyId == id)
                {
                    return true;
                }

                return w.Name == id;
            });
            
            if (exists != null)
            {
                current.Remove(exists);
            }

            await _mediator.Send(new SaveUserJsonDataCommand
                {Key = SaveProxyKey, Value = current, UserId = userId});
        }

        public async Task SaveProxy(int userId, RsClientProxy proxy)
        {
            proxy.Date = DateTimeOffset.UtcNow;

            if (string.IsNullOrEmpty(proxy.Name))
            {
                throw new Exception("Proxy name is required.");
            }

            if (string.IsNullOrEmpty(proxy.Ip) || string.IsNullOrEmpty(proxy.Port))
            {
                throw new Exception("Proxy Ip and Port are required.");
            }

            if (proxy.Name.Length > 100)
            {
                throw new Exception("Proxy name must be under 100 characters.");
            }

            var current = (await GetProxies(userId)).ToList();

            var exists = current.FirstOrDefault(w =>
            {
                if (w.ProxyId != null && proxy.ProxyId != null && w.ProxyId == proxy.ProxyId)
                {
                    return true;
                }

                return w.Name == proxy.Name;
            });

            if (exists == null)
            {
                proxy.ProxyId = Guid.NewGuid().ToString();
                current.Add(proxy);
            }
            else
            {
                current.Remove(exists);
                current.Add(proxy);
            }

            if (current.Count >= 500) throw new Exception("You may not save more than 500 proxies.");

            await _mediator.Send(new SaveUserJsonDataCommand
                {Key = SaveProxyKey, Value = current, UserId = userId});
        }

        public async Task<IEnumerable<RsClientProxy>> GetProxies(int userId)
        {
            var result = await _mediator.Send(new GetUserJsonDataQuery
            {
                UserId = userId,
                Key = SaveProxyKey
            });
            return result.To<List<RsClientProxy>>()?.OrderBy(w => w.Name).ToList() ?? new List<RsClientProxy>();
        }
    }
}