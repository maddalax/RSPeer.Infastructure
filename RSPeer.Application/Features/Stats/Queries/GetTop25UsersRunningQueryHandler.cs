using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Stats.Models;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Stats.Queries
{
	public class GetTop25UsersRunningQueryHandler : IRequestHandler<GetTop25UsersRunningQuery, IEnumerable<UserAndCountResult>>
	{
		private readonly RsPeerContext _db;

		public GetTop25UsersRunningQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<UserAndCountResult>> Handle(GetTop25UsersRunningQuery request,
			CancellationToken cancellationToken)
		{
			return await _db.Database.GetDbConnection().QueryAsync<UserAndCountResult>(
				$@"SELECT distinct(userid), username, count(*) from runescapeclients
				inner join users on users.id = userid
				where lastupdate > current_timestamp - interval '5 minute'
				group by userid, username
				order by count(*) desc
				limit 25");
		}
	}
}