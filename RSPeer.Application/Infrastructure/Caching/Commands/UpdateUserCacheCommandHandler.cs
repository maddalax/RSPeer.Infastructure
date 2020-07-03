using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;
using StackExchange.Redis;

namespace RSPeer.Application.Infrastructure.Caching.Commands
{
    public class UpdateUserCacheCommandHandler : IRequestHandler<UpdateUserCacheCommand>
    {
        private readonly IRedisService _redis;
        private readonly RsPeerContext _db;

        public UpdateUserCacheCommandHandler(IRedisService redis, RsPeerContext db)
        {
            _redis = redis;
            _db = db;
        }

        public async Task<Unit> Handle(UpdateUserCacheCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(w => w.UserGroups).ThenInclude(w => w.Group)
                .FirstOrDefaultAsync(w => w.Username.ToLower() == request.UsernameOrEmail.ToLower() 
                                          || w.Email.ToLower() == request.UsernameOrEmail.ToLower(), cancellationToken);

            if (user == null)
            {
                throw new ArgumentException("User was not found by value: " + request.UsernameOrEmail);
            }
            
            var transaction = _redis.GetDatabase().CreateTransaction();
            var serialized = JsonSerializer.Serialize(user);
            transaction.StringSetAsync($"user_{user.Id}", serialized);
            if (user.LinkKey != null)
            {
                transaction.StringSetAsync($"{user.LinkKey}_user_id", user.Id);
            }
            transaction.StringSetAsync($"user_{user.Username}", serialized);
            await transaction.ExecuteAsync(CommandFlags.FireAndForget);
            return Unit.Value;
        }
    }
}