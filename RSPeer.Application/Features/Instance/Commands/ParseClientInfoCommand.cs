using System;
using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Instance.Commands
{
	public class ParseClientInfoCommand : IRequest<RunescapeClient>
	{
		public string RawClientInfo { get; set; }
		public int UserId { get; set; }
		public Guid Identifier { get; set; }

		public Game Game { get; set; } = Game.Osrs;
		
		public bool IsXor { get; set; }
	}
}