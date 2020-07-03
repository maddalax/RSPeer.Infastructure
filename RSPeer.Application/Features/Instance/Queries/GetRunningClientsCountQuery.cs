using MediatR;

namespace RSPeer.Application.Features.Instance.Queries
{
	public class GetRunningClientsCountQuery : IRequest<long>
	{
		public int UserId { get; set; }
	}
}