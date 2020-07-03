using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
    public class GetDiscordAccountQueryHandler : IRequestHandler<GetDiscordAccountQuery, DiscordAccount>
    {
        private readonly RsPeerContext _db;
        private readonly IMediator _mediator;

        public GetDiscordAccountQueryHandler(RsPeerContext db, IMediator mediator)
        {
            _db = db;
            _mediator = mediator;
        }

        public async Task<DiscordAccount> Handle(GetDiscordAccountQuery request, CancellationToken cancellationToken)
        {
            if (request.DiscordId != null)
            {
                return await _db.DiscordAccounts.FirstOrDefaultAsync(w => w.DiscordUserId == request.DiscordId,
                    cancellationToken);
            }

            if (request.Username == null)
            {
                return null;
            }

            var user = await _mediator.Send(new GetUserByUsernameQuery {Username = request.Username}, cancellationToken);

            if (user == null)
            {
                return null;
            }

            return await _db.DiscordAccounts.FirstOrDefaultAsync(w => w.UserId == user.Id, cancellationToken);
        }
    }
}