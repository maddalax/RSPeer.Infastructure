using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RSPeer.Application.Infrastructure.Models;
using RSPeer.Domain.Entities;
using RSPeer.Persistence;

namespace RSPeer.Application.Infrastructure.Caching.Listeners
{
	public class RedisPubSubCommandHandler : IRedisPubSubListener
	{
		private readonly IServiceScopeFactory _factory;

		public RedisPubSubCommandHandler(IServiceScopeFactory factory)
		{
			_factory = factory;
		} 
		
		public async Task OnMessage(string channel, string message)
		{
			using (var scope = _factory.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<RsPeerContext>();
				await db.Data.AddAsync(new Data
				{
					Key = $"redis:pubsub:{channel}",
					Value = message
				});
				await db.SaveChangesAsync();		
			}
		}
	}
}