using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Infrastructure.Caching.Base;

namespace RSPeer.Application.Features.Instance.Commands
{
	public class SetClientClosedCommandHandler : IRequestHandler<SetClientClosedCommand, Unit>
	{
		private readonly IRedisService _redis;

		public SetClientClosedCommandHandler(IRedisService redis)
		{
			_redis = redis;
		}

		public async Task<Unit> Handle(SetClientClosedCommand request, CancellationToken cancellationToken)
		{
			var transaction = _redis.GetDatabase().CreateTransaction();
			transaction.KeyDeleteAsync($"{request.Tag}_client_details");
			await transaction.ExecuteAsync();
			return Unit.Value;
		}
	}
}