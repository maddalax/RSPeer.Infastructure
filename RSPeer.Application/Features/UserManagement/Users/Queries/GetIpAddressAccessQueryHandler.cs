using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
	public class GetIpAddressAccessQueryHandler : IRequestHandler<GetIpAddressAccessQuery, IEnumerable<string>>
	{
		private readonly RsPeerContext _db;

		public GetIpAddressAccessQueryHandler(RsPeerContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<string>> Handle(GetIpAddressAccessQuery request, CancellationToken cancellationToken)
		{
			var key = $"ipAddress:access:{request.UserId}";
			return await _db.Data.Where(w => w.Key == key).Select(w => w.Value).ToListAsync(cancellationToken);
		}
	}
}