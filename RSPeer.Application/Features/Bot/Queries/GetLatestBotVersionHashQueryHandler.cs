using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPeer.Application.Features.Files.Queries;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Bot.Queries
{
	public class GetLatestBotVersionHashQueryHandler : IRequestHandler<GetLatestBotVersionHashQuery, string>
	{
		private readonly RsPeerContext _db;
		private readonly IMediator _mediator;

		public GetLatestBotVersionHashQueryHandler(RsPeerContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<string> Handle(GetLatestBotVersionHashQuery request, CancellationToken cancellationToken)
		{
			var version = await _mediator.Send(new GetFileLatestVersionQuery { Name = "rspeer.jar" }, cancellationToken);
			var formatted = version.ToString("F");
			var result = await _db.Data.FirstOrDefaultAsync(w => w.Key.Equals($"bot:version:hash:{formatted}"), 
				cancellationToken);
			return result.Value;
		}
	}
}