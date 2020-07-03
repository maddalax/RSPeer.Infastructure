using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.Scripts.Commands.CompileScript.Models
{
	public class CompileRequest
	{
		public string GitPath { get; set; }
		public string ObfuscateConfig { get; set; }
		
		public Game Game { get; set; }
	}
}