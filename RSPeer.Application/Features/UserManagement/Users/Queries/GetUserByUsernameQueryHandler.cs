using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
	public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, User>
	{
		private readonly RsPeerContext _db;
		private readonly IRedisService _redis;
		private readonly IMediator _mediator;

		public GetUserByUsernameQueryHandler(RsPeerContext db, IRedisService redis, IMediator mediator)
		{
			_db = db;
			_redis = redis;
			_mediator = mediator;
		}

		public async Task<User> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(request.Username))
			{
				return null;
			}

			return await _redis.GetOrDefault($"user_{request.Username}", async () =>
			{
				var queryable = _db.Users.AsQueryable().Where(w => w.Username.ToLower() == request.Username.ToLower().Trim());
				queryable = queryable.Include(w => w.UserGroups)
					.ThenInclude(w => w.Group);
				var user = await queryable.FirstOrDefaultAsync(cancellationToken);
				if (user == null)
				{
					return null;
				}

				return await _mediator.Send(new GetUserByIdQuery {Id = user.Id}, cancellationToken);
			});

		}
	}
}