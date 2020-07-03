using MediatR;

namespace RSPeer.Application.Features.Files.Queries
{
	public class GetFileLatestVersionQuery : IRequest<decimal>
	{
		public string Name { get; set; }
	}
}