using MediatR;
using RSPeer.Domain.Dtos;

namespace RSPeer.Application.Features.Scripts.Queries.GetScript
{
	public class GetScriptByIdQuery : IRequest<ScriptDto>
	{
		public int ScriptId { get; set; }
		public bool IncludeDeveloper { get; set; }
	}
}