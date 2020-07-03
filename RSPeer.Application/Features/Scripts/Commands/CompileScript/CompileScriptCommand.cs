using MediatR;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripts.Commands.CompileScript
{
	public class CompileScriptCommand : IRequest<CompiledScript>
	{
		public string GitlabUrl { get; set; }
		public Game Game { get; set; }
	}
}