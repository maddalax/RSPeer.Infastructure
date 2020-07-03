using System;
using System.Text.Json.Serialization;

namespace RSPeer.Domain.Entities
{
	public class ScriptAccess
	{
		public int Id { get; set; }
		
		public string LegacyId { get; set; }
		public int UserId { get; set; }
		
		[JsonIgnore]
		public User User { get; set; }
		
		public int ScriptId { get; set; }
		
		[JsonIgnore]
		public Script Script { get; set; }
		
		public int? OrderId { get; set; }
		public DateTimeOffset Timestamp { get; set; }
		public DateTimeOffset? Expiration { get; set; }
		public int? Instances { get; set; }
		public bool Recurring { get; set; }
		
		public int? AdminUserId { get; set; }
	}
}