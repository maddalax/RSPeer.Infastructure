using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Groups.Queries
{
    public class HasGroupQueryHandler : IRequestHandler<HasGroupQuery, bool>
    {
        private readonly RsPeerContext _db;

        public HasGroupQueryHandler(RsPeerContext db)
        {
            _db = db;
        }

        public async Task<bool> Handle(HasGroupQuery request, CancellationToken cancellationToken)
        {
            return await _db.UserGroups
                .AnyAsync(w => request.GroupIds.Contains(w.GroupId) && w.UserId == request.UserId, cancellationToken);
        }
    }
}