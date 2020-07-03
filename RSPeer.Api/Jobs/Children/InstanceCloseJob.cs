using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Discord.Commands;
using RSPeer.Application.Features.Discord.Models;
using RSPeer.Application.Features.Instance.Queries;
using RSPeer.Application.Features.Migration.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;
using CommandFlags = StackExchange.Redis.CommandFlags;

namespace RSPeer.Api.Jobs.Children
{
    public class InstanceCloseJob
    {
        private readonly IMediator _mediator;
        private readonly IRedisService _redis;
        private int count;
        private HashSet<int> users;

        public InstanceCloseJob(IMediator mediator, IRedisService redis)
        {
            _mediator = mediator;
            _redis = redis;
            users = new HashSet<int>();
        }

        public async Task Execute()
        {
            var enabled = await _mediator.Send(new IsReadOnlyModeQuery());
			
            if (enabled)
            {
                return;
            }
            
            var users = await _redis.GetSet($"users_running_clients_{Game.Osrs.ToString()}");
            
            foreach (var user in users)
            {
                await CheckInstances(int.Parse(user)); 
            }

            if (count > 0)
            {
                await _mediator.Send(new SendDiscordWebHookCommand
                {
                    Type = DiscordWebHookType.Log,
                    Message = $"Successfully set {count} accounts marked for over instance limit. Users affected: " + JsonSerializer.Serialize(users)
                });
            }
        }

        private async Task CheckInstances(int userId)
        {
            var allowed = await _mediator.Send(new GetAllowedInstancesQuery {UserId = userId});

            var running = await _mediator.Send(new GetRunningClientsQuery {UserId = userId});

            var tags = running
                .Where(w => w.Game == Game.Osrs).Select(w => w.Tag)
                .OrderByDescending(w => w)
                .Distinct().ToList();
            
            var allowedTags = tags.Take(allowed).ToHashSet();
            var overLimit = tags.Where(w => !allowedTags.Contains(w)).ToArray();
            var transaction = _redis.GetDatabase().CreateTransaction();
            
            foreach (var allowedTag in allowedTags)
            {
                transaction.StringSetAsync($"clients_over_limit_{allowedTag}", false, TimeSpan.FromMinutes(15));
            }
            
            foreach (var tag in overLimit)
            {
                count++;
                users.Add(userId);
                transaction.StringSetAsync($"clients_over_limit_{tag}", true, TimeSpan.FromMinutes(15));
            }

            await transaction.ExecuteAsync(CommandFlags.FireAndForget);
        }
    }
}