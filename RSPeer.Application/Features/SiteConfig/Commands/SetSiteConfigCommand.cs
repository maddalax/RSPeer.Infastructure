using MediatR;

namespace RSPeer.Application.Features.SiteConfig.Commands
{
	public class SetSiteConfigCommand : IRequest<Unit>
	{
		public string Key { get; set; }
		public string Value { get; set; }
	}
}