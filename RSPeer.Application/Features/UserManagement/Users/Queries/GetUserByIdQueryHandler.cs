using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Instance.Queries;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, User>
    {
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;
        private readonly IMediator _mediator;

        public GetUserByIdQueryHandler(RsPeerContext db, IRedisService redis, IMediator mediator)
        {
            _db = db;
            _redis = redis;
            _mediator = mediator;
        }

        public async Task<User> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await _redis.GetOrDefault($"user_{request.Id}", async () =>
                {
                    var queryable = _db.Users.AsQueryable()
                        .Include(w => w.UserGroups)
                        .ThenInclude(w => w.Group)
                        .Where(w => w.Id == request.Id);
                    var user = await queryable.FirstOrDefaultAsync(cancellationToken);
                    if (user == null)
                    {
                        return null;
                    }
                    var allowed = await _mediator.Send(new GetAllowedInstancesQuery {UserId = user.Id},
                        cancellationToken);
                    user.Instances = allowed;
                    return user;
                },
                TimeSpan.FromDays(365));
        }
    }
}