using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripts.Queries.CreateScript
{
	public class GetScripterInfoQuery : IRequest<ScripterInfo>
	{
		public int UserId { get; set; }
	}
}