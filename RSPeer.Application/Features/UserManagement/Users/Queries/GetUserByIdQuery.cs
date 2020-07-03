using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserManagement.Users.Queries
{
	public class GetUserByIdQuery : IRequest<User>
	{
		public int Id { get; set; }
		public bool IncludeGroups { get; set; }
		public bool AllowCached { get; set; }
		
		public bool IncludeInstances { get; set; }
	}
}