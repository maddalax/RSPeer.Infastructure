using System;
using System.Text.Json.Serialization;

namespace RSPeer.Domain.Entities
{
	public class PrivateScriptAccess
	{
		public int Id { get; set; }
		public int ScriptId { get; set; }
		
		[JsonIgnore]
		public Script Script { get; set; }
		public User User { get; set; }
		public int UserId { get; set; }
		
		public DateTimeOffset Timestamp { get; set; }
	}
}