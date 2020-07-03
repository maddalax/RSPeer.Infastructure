using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSPeer.Domain.Entities
{
	public class RunescapeClient
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public DateTimeOffset LastUpdate { get; set; }
		public string Ip { get; set; }
		public string ProxyIp { get; set; }
		public string MachineName { get; set; }
		public string OperatingSystem { get; set; }
		public string ScriptName { get; set; }
		public string Rsn { get; set; }
		public string RunescapeEmail { get; set; }

		public bool IsManuallyClosed { get; set; }

		public Guid Tag { get; set; }
		
		public string ScriptId { get; set; }

		public bool IsRepoScript { get; set; }
		
		public string JavaVersion { get; set; }
		
		public string MachineUserName { get; set; }
		
		public string ScriptClassName { get; set; }
		
		public string ScriptDeveloper { get; set; } 
		
		public string Hash { get; set; }
		
		public decimal? Version { get; set; }
		
		public bool? AtInstanceLimit { get; set; }

		[Column(TypeName = "jsonb")]
		public string RspeerFolderFileNames { get; set; }
		
		public bool IsBanned { get; set; }

		public Game Game { get; set; } = Game.Osrs;
	}
}