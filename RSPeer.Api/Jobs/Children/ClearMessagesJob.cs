using System;
using System.Linq;
using System.Threading.Tasks;
using RSPeer.Persistence;

namespace RSPeer.Api.Jobs.Children
{
    public class ClearMessagesJob
    {
        private readonly RsPeerContext _db;

        public ClearMessagesJob(RsPeerContext db)
        {
            _db = db;
        }

        public async Task Execute()
        {
            var expiration = DateTimeOffset.UtcNow.AddMinutes(-5);
            _db.Messages.RemoveRange(_db.Messages.Where(w => w.Timestamp < expiration));
            _db.Launchers.RemoveRange(_db.Launchers.Where(w => w.LastUpdate < expiration));
            await _db.SaveChangesAsync();
        }
    }
}