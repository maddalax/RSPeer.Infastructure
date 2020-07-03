using System.Collections.Generic;

namespace RSPeer.Domain.Entities
{
	public class ClientInfo
	{
		public string Email { get; set; }

		public int UserId { get; set; }

		public string IpAddress { get; set; }

		public string ProxyIp { get; set; }

		public string ProxyUsername { get; set; }

		public string ScriptName { get; set; }

		public int ScriptId { get; set; }

		public bool IsRepoScript { get; set; }

		public string Rsn { get; set; }

		public string MachineName { get; set; }

		public string JavaVersion { get; set; }

		public string OperatingSystem { get; set; }

		public string MachineUserName { get; set; }
		
		public string ScriptClassName { get; set; }
		
		public string ScriptDeveloper { get; set; } 
		
		public string RunescapeLogin { get; set; }
		
		public decimal Version { get; set; }
		
		public bool AtInstanceLimit { get; set; }
		
		public bool IsBanned { get; set; }

		public IEnumerable<string> RspeerFolderFileNames = new List<string>();
	}
}