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
    public class DeleteApiClientCommandHandler : IRequestHandler<DeleteApiClientCommand, Unit>
    {
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;

        public DeleteApiClientCommandHandler(RsPeerContext db, IRedisService redis)
        {
            _db = db;
            _redis = redis;
        }

        public async Task<Unit> Handle(DeleteApiClientCommand request, CancellationToken cancellationToken)
        {
            var exists = await _db.ApiClients.FirstOrDefaultAsync(w => w.UserId == request.User.Id, cancellationToken);
            if (exists == null)
            {
                return Unit.Value;
            }
            _db.ApiClients.Remove(exists);
            await _db.SaveChangesAsync(cancellationToken);
            await _redis.Remove($"{request.User.Id}_api_client");
            return Unit.Value;
        }

      
    }
}