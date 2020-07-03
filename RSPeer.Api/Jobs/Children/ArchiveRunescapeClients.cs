using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using RSPeer.Persistence;

namespace RSPeer.Api.Jobs.Children
{
    public class ArchiveRunescapeClients
    {
        private readonly RsPeerContext _db;

        public ArchiveRunescapeClients(RsPeerContext db)
        {
            _db = db;
        }

        public async Task Execute()
        {
            /*
            await _db.Database.GetDbConnection().ExecuteAsync(@"with delta as (
                    delete from runescapeclients where lastupdate < current_timestamp - INTERVAL '1 Month'
                    returning *)
                    insert into runescapeclients_archive (select * from delta);");
                    */
        }
    }
}