using System.Collections.Generic;
using MediatR;
using RSPeer.Domain.Dtos;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripts.Queries.GetScript
{
	public class GetScriptsQuery : IRequest<IEnumerable<ScriptDto>>
	{
		public ScriptType? Type { get; set; }
		public string Search { get; set; }
		public ScriptOrderBy OrderBy { get; set; }
		public ScriptCategory? Category { get; set; }
		
		public ScriptStatus? Status { get; set; }
		public int? UserId { get; set; }

		public Game Game { get; set; } = Game.Osrs;
	}

	public enum ScriptOrderBy
	{
		Featured,
		FeaturedAllTime,
		LastUpdated,
		Users,
		Alphabetical,
		Newest
	}
}