using MediatR;

namespace RSPeer.Application.Features.Files.Queries
{
	public class GetLatestFileVersionQuery : IRequest<decimal>
	{
		public string Name { get; set; }
	}
}