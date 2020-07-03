using System;
using RSPeer.Domain.Entities;

namespace RSPeer.Domain.Dtos
{
	public class PrivateScriptAccessDto
	{
		public int UserId { get; set; }
		public string Username { get; set; }
		public int ScriptId { get; set; }
		
		public DateTimeOffset Timestamp { get; set; }

		public PrivateScriptAccessDto(PrivateScriptAccess access)
		{
			UserId = access.User?.Id ?? 0;
			Username = access.User?.Username;
			ScriptId = access.ScriptId;
			Timestamp = access.Timestamp;
		}

		public PrivateScriptAccessDto()
		{
			
		}
	}
}