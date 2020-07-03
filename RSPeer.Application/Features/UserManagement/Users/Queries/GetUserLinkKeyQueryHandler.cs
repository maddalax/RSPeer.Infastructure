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
    public class GetUserLinkKeyQueryHandler : IRequestHandler<GetUserLinkKeyQuery, Guid?>
    {
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;

        public GetUserLinkKeyQueryHandler(RsPeerContext db, IRedisService redis)
        {
            _db = db;
            _redis = redis;
        }

        public async Task<Guid?> Handle(GetUserLinkKeyQuery request, CancellationToken cancellationToken)
        {
            return await _redis.GetOrDefault($"{request.UserId}_link_key", async () =>
                {
                    return await _db.Users.Where(w => w.Id == request.UserId).Select(w => w.LinkKey)
                        .FirstOrDefaultAsync(cancellationToken);
                }, 
                TimeSpan.FromDays(30));
        }
    }
}