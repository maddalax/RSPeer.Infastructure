using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Instance.Commands
{
	public class SaveClientInfoCommand
	{
		public RunescapeClient Client { get; set; }
	}
}