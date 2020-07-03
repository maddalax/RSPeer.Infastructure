using MediatR;

namespace RSPeer.Application.Features.Files.Queries
{
	public class GetFileQuery : IRequest<byte[]>
	{
		public string Name { get; set; }
		public decimal? Version { get; set; }
	}
}