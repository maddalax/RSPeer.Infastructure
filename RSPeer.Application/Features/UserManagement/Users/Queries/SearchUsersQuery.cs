using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
	public class SearchUsersQuery : IRequest<IEnumerable<User>>
	{
		public string SearchTerm { get; set; } 
	}
}