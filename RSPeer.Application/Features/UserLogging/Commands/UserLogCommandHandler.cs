using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserLogging.Commands
{
    public class UserLogCommandHandler : IRequestHandler<UserLogCommand, Unit>
    {
        private readonly RsPeerContext _db;

        public UserLogCommandHandler(RsPeerContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(UserLogCommand request, CancellationToken cancellationToken)
        {
            var log = new UserLog
            {
                Message = request.Message,
                Timestamp = DateTimeOffset.Now,
                UserId = request.UserId,
                Type = request.Type
            };
            await _db.UserLogs.AddAsync(log, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}