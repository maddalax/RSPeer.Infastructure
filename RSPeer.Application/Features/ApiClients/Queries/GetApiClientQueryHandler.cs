using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.ApiClients.Queries
{
    public class GetApiClientQueryHandler : IRequestHandler<GetApiClientQuery, ApiClient>
    {
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;

        public GetApiClientQueryHandler(RsPeerContext db, IRedisService redis)
        {
            _db = db;
            _redis = redis;
        }

        public async Task<ApiClient> Handle(GetApiClientQuery request, CancellationToken cancellationToken)
        {
            return await _redis.GetOrDefault($"{request.User.Id}_api_client", async () =>
            {
                return await _db.ApiClients.FirstOrDefaultAsync(w => w.UserId == request.User.Id, 
                    cancellationToken);
            });
        }
    }
}