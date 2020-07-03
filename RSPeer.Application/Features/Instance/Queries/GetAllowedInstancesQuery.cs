using MediatR;

namespace RSPeer.Application.Features.Instance.Queries
{
	public class GetAllowedInstancesQuery : IRequest<int>
	{
		public int UserId { get; set; }
		public bool IncludeFree { get; set; } = true;
	}
}