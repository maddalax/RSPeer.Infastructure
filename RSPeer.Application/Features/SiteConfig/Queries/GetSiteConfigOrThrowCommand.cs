using MediatR;

namespace RSPeer.Application.Features.SiteConfig.Queries
{
	public class GetSiteConfigOrThrowCommand : IRequest<string>
	{
		public string Key { get; set; }
	}
}