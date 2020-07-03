using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Infrastructure.Caching.Base;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserData.Commands
{
    public class SaveUserJsonDataCommandHandler : IRequestHandler<SaveUserJsonDataCommand, Unit>
    {
        private readonly RsPeerContext _db;
        private readonly IRedisService _redis;

        public SaveUserJsonDataCommandHandler(RsPeerContext db, IRedisService redis)
        {
            _db = db;
            _redis = redis;
        }

        public async Task<Unit> Handle(SaveUserJsonDataCommand request, CancellationToken cancellationToken)
        {
            var exists =
                await _db.UserJsonData.FirstOrDefaultAsync(w => w.Key == request.Key && w.UserId == request.UserId,
                    cancellationToken);

            var value = JsonSerializer.Serialize(request.Value);

            if (exists != null)
            {
                exists.Value = value;
                _db.UserJsonData.Update(exists);
            }
            else
            {
                exists = new UserJsonData {Key = request.Key, Value = value, UserId = request.UserId};
                await _db.UserJsonData.AddAsync(exists, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            await _redis.GetDatabase().StringSetAsync($"user_json_data_{request.Key}_{request.UserId}",
                JsonSerializer.Serialize(exists));
            return Unit.Value;
        }
    }
}