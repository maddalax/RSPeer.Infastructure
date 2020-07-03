using System;
using RSPeer.Domain.Entities;

namespace RSPeer.Domain.Dtos
{
	public class RunescapeClientDto
	{
		public DateTimeOffset LastUpdate { get; set; }
		public string ProxyIp { get; set; }
		public string MachineName { get; set; }
		public string ScriptName { get; set; }
		public string Rsn { get; set; }
		public string RunescapeEmail { get; set; }
		public string Tag { get; set; }
		public Game Game { get; set; }
	}
}