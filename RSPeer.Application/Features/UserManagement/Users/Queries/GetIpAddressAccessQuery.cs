using System.Collections.Generic;
using MediatR;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
	public class GetIpAddressAccessQuery : IRequest<IEnumerable<string>>
	{
		public int UserId { get; set; }
	}
}