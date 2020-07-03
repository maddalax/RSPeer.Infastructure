using System.Threading.Tasks;

namespace RSPeer.Application.Infrastructure.Models
{
	public interface IRedisPubSubListener
	{
		Task OnMessage(string channel, string message);
	}
}