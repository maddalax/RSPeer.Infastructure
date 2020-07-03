using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
    public class GetUserIdByLinkKeyQueryHandler : IRequestHandler<GetUserIdByLinkKeyQuery, int>
    {
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;

        public GetUserIdByLinkKeyQueryHandler(RsPeerContext db, IRedisService redis)
        {
            _db = db;
            _redis = redis;
        }

        public async Task<int> Handle(GetUserIdByLinkKeyQuery request, CancellationToken cancellationToken)
        {
            return await _redis.GetOrDefault($"{request.LinkKey}_user_id", async () =>
                {
                    return await _db.Users.Where(w => w.LinkKey == request.LinkKey).Select(w => w.Id)
                        .FirstOrDefaultAsync(cancellationToken);
                }, 
                TimeSpan.FromDays(30));
        }
    }
}