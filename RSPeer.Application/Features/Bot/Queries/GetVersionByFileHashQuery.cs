using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Bot.Queries
{
	public class GetVersionByFileHashQuery : IRequest<decimal>
	{
		public string Hash { get; set; }
		public Game Game { get; set; } = Game.Osrs;
	}
}