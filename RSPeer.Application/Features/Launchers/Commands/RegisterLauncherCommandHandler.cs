using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.UserManagement.Users.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;
using StackExchange.Redis;

namespace RSPeer.Application.Features.Launchers.Commands
{
    public class RegisterLauncherCommandHandler : IRequestHandler<RegisterLauncherCommand, Unit>
    {
        private readonly IRedisService _redis;
        private readonly IMediator _mediator;

        public RegisterLauncherCommandHandler(IRedisService redis, IMediator mediator)
        {
            _redis = redis;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(RegisterLauncherCommand request, CancellationToken cancellationToken)
        {
            if (!request.UserId.HasValue && request.LinkKey.HasValue)
            {
                var userId = await _mediator.Send(new GetUserIdByLinkKeyQuery {LinkKey = request.LinkKey.Value}, cancellationToken);
                request.UserId = userId;
            }

            if (!request.UserId.HasValue)
            {
                throw new Exception("Failed to get user by link key or user id, cannot register launcher.");
            }

            var transaction = _redis.GetDatabase().CreateTransaction();

            var key = $"{request.UserId}_{request.Tag}_launcher";

            var launcher = new Domain.Entities.Launcher
            {
                Ip = request.Ip,
                UserId = request.UserId.Value,
                Tag = request.Tag,
                Host = request.Host,
                MachineUsername = request.MachineUsername,
                Platform = request.Platform,
                LastUpdate = DateTimeOffset.Now
            };

            transaction.StringSetAsync(key, JsonSerializer.Serialize(launcher), 
                TimeSpan.FromMinutes(3));

            transaction.SetAddAsync($"{request.UserId}_connected_launchers", request.Tag.ToString());

            await transaction.ExecuteAsync(CommandFlags.FireAndForget);
            return Unit.Value;
        }
    }
}