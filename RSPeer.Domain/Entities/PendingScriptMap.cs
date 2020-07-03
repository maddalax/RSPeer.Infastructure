using System.Text.Json.Serialization;

namespace RSPeer.Domain.Entities
{
	public class PendingScriptMap
	{
		public int Id { get; set; }
		public int LiveScriptId { get; set; }
		public int PendingScriptId { get; set; }
		
		public int? DeniedBy { get; set; }
		
		[JsonIgnore]
		public Script PendingScript { get; set; }
		
		public ScriptStatus Status { get; set; }
		
		public string Message { get; set; }
	}
}