using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ApiClients.Queries
{
    public class GetApiClientByKeyQueryHandler : IRequestHandler<GetApiClientByKeyQuery, ApiClient>
    {
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;

        public GetApiClientByKeyQueryHandler(RsPeerContext db, IRedisService redis)
        {
            _db = db;
            _redis = redis;
        }

        public async Task<ApiClient> Handle(GetApiClientByKeyQuery request, CancellationToken cancellationToken)
        {
            return await _redis.GetOrDefault($"{request.Key}_api_client_by_key", async () =>
            {
                return await _db.ApiClients.FirstOrDefaultAsync(w => w.Key == request.Key, 
                    cancellationToken);
            });
        }
    }
}