using System;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ApiClients.Commands
{
    public class CreateApiClientCommandHandler : IRequestHandler<CreateApiClientCommand, ApiClient>
    {
        private RsPeerContext _db;
        private readonly IRedisService _redis;

        public CreateApiClientCommandHandler(RsPeerContext db, IRedisService redis)
        {
            _db = db;
            _redis = redis;
        }

        public async Task<ApiClient> Handle(CreateApiClientCommand request, CancellationToken cancellationToken)
        {
            var exists = await _db.ApiClients.FirstOrDefaultAsync(w => w.UserId == request.User.Id, cancellationToken);

            if (exists != null)
            {
                throw new Exception("You already have an api client.");
            }
            
            var key = await GenerateKey();
            var client = new ApiClient
            {
                Key = key,
                UserId = request.User.Id,
                Timestamp = DateTimeOffset.UtcNow
            };
            await _db.ApiClients.AddAsync(client, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await _redis.SetJson($"{request.User.Id}_api_client", client);
            await _redis.SetJson($"{key}_api_client_by_key", client);
            return client;
        }

        private async Task<string> GenerateKey(int tries = 0)
        {
            if (tries > 50)
            {
                throw new Exception("Failed to generate key after 50 tries.");
            } 
            
            var key = RandomString(70);
            
            var exists = await _db.ApiClients.FirstOrDefaultAsync(w => w.Key == key);
            
            if (exists != null)
            {
                return await GenerateKey(tries + 1);
            }

            return key;
        }
        
        private static readonly Random Random = new Random();
        
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}