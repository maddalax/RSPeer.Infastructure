using System;
using MediatR;

namespace RSPeer.Application.Features.Instance.Commands
{
	public class SetClientClosedCommand : IRequest<Unit>
	{
		public Guid Tag { get; set; }
		public int UserId { get; set; }
	}
}