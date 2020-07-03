using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Discord.Commands;
using RSPeer.Application.Features.Discord.Setup.Base;
using RSPeer.Application.Features.Store.Process.Queries;
using RSPeer.Application.Features.UserData.Queries;
using RSPeer.Application.Features.UserLogging.Commands;
using RSPeer.Domain.Constants;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Api.Jobs.Children
{
    public class SendDiscordAlerts
    {
        private readonly IDiscordSocketClientProvider _provider;
        private readonly RsPeerContext _db;
        private readonly IMediator _mediator;

        public SendDiscordAlerts(IDiscordSocketClientProvider provider, RsPeerContext db, IMediator mediator)
        {
            _provider = provider;
            _db = db;
            _mediator = mediator;
        }

        public async Task Execute()
        {
            var client = await _provider.Get();
            await CheckExpiredInstances(client);
            await CheckExpiredScripts(client);
            await CheckExpiredInuvation(client);
        }

        private async Task CheckExpiredInstances(DiscordSocketClient client)
        {
            var monthAgo = DateTimeOffset.UtcNow.AddDays(-32);
            var orders = await _db.Orders.Include(w => w.Item)
                .OrderByDescending(w => w.Timestamp)
                .Where(w => w.Item.Sku == "instances")
                .Where(w => w.Timestamp > monthAgo).ToListAsync();

            var filtered = (await FilterAlmostExpired(orders)).ToList();
            
            foreach (var record in filtered)
            {
                var order = record.order;
                var diff = record.expiration;
            
                var checkout = $"https://app.rspeer.org/#/store/checkout?sku=instances&quantity={order.Quantity}";

                var message =
                    $"Your order ({order.Id}) of {order.Quantity} instances is expiring in {diff.Days} days and {diff.Hours} hours. To ensure you are able to run your clients, renew now. {checkout}";

                if (!await AllowsAlerts(order.UserId))
                {
                    continue;
                }
                
                var sent = await _mediator.Send(new SendDiscordPrivateMessageCommand
                {
                    Client = client,
                    Message = message,
                    UserId = order.UserId
                });

                if (sent)
                {
                    await _mediator.Send(new UserLogCommand
                    {
                        Message = message,
                        Type = record.key,
                        UserId = order.UserId
                    });
                }
            }
        }
        
        private async Task CheckExpiredScripts(DiscordSocketClient client)
        {
            var monthAgo = DateTimeOffset.UtcNow.AddDays(-32);
            var orders = await _db.Orders.Include(w => w.Item)
                .OrderByDescending(w => w.Timestamp)
                .Where(w => w.Item.Sku.Contains("premium-script-"))
                .Where(w => w.Timestamp > monthAgo).ToListAsync();

            var filtered = (await FilterAlmostExpired(orders)).ToList();
            
            foreach (var record in filtered)
            {
                var order = record.order;
                var diff = record.expiration;
                var checkout = $"https://app.rspeer.org/#/store/checkout?sku={order.Item.Sku}&quantity={order.Quantity}";

                var message =
                    $"Your order ({order.Id}) of {order.Item.Name} - {order.Item.Description} is expiring in {diff.Days} days and {diff.Hours} hours. To ensure you do not lose access, renew now. {checkout}.";

                if (!await AllowsAlerts(order.UserId))
                {
                    continue;
                }
                
                var sent = await _mediator.Send(new SendDiscordPrivateMessageCommand
                {
                    Client = client,
                    Message = message,
                    UserId = order.UserId
                });

                if (sent)
                {
                    await _mediator.Send(new UserLogCommand
                    {
                        Message = message,
                        Type = record.key,
                        UserId = order.UserId
                    });
                }
            }
        }
        
        private async Task CheckExpiredInuvation(DiscordSocketClient client)
        {
            var groups = await _db.UserGroups.Where(w => w.GroupId == GroupConstants.InuvationId).ToListAsync();
            foreach (var group in groups)
            {
                var orders = await _mediator.Send(new GetOrdersQuery
                    {Sku = "rs3-inuvation-access", UserId = group.UserId, NotExpired = true, Status = OrderStatus.Completed, IncludeItem = true});
                
                foreach (var order in orders)
                {
                    var expiration = order.Timestamp.AddMinutes(order.Item.ExpirationInMinutes.GetValueOrDefault(43200));
                    var diff = expiration - DateTimeOffset.UtcNow;
                    
                    if (diff.Days > 3)
                    {
                        continue;
                    }
                    var checkout = "https://app.rspeer.org/#/store/inuvation";
                
                    var message =
                        $"Your RSPeer RS3 Inuvation Access is expiring in {diff.Days} days and {diff.Hours} hours. To ensure you do not lose access, renew now. {checkout}.";

                    var key = $"order:expiration:alert:inuvation:{order.Id}";

                    // Already sent a notification for this order.
                    if (await _db.UserLogs.AnyAsync(w => w.Type == key))
                    {
                        continue;
                    }

                    if (!await AllowsAlerts(group.UserId))
                    {
                        continue;
                    }
                
                    var sent = await _mediator.Send(new SendDiscordPrivateMessageCommand
                    {
                        Client = client,
                        Message = message,
                        UserId = group.UserId
                    });

                    if (sent)
                    {
                        await _mediator.Send(new UserLogCommand
                        {
                            Message = message,
                            Type = key,
                            UserId = group.UserId
                        });
                    }
                }
            }
        }

        private async Task<bool> AllowsAlerts(int userId)
        {
            var allows = await _mediator.Send(new GetUserJsonDataQuery {Key = "discord_allow_alerts", UserId = userId});
            return allows == null || bool.Parse(allows);
        }
        
        private async Task<List<(Order order, TimeSpan expiration, string key)>> FilterAlmostExpired(IEnumerable<Order> orders)
        {
            var result = new List<(Order order, TimeSpan expiration, string key)>();
            foreach (var order in orders)
            {
                var expiration = order.Item.ExpirationInMinutes.GetValueOrDefault(43200);
               
                var finalExpiration = order.Timestamp.AddMinutes(expiration);
                var diff = finalExpiration - DateTimeOffset.UtcNow;

                if (diff.Days > 2)
                {
                    continue;
                }

                var key = $"order:expiration:alert:{order.Id}";

                // Already sent a notification for this order.
                if (await _db.UserLogs.AnyAsync(w => w.Type == key))
                {
                    continue;
                }

                result.Add((order, diff, key));
            }

            return result;
        }
    }
}