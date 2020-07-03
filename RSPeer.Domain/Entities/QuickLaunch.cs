using System;
using System.Collections.Generic;

namespace RSPeer.Domain.Entities
{
	public class QuickLaunch
	{
		public string QuickLaunchId { get; set; }
		public string Name { get; set; }
		public List<RsClient> Clients { get; set; } = new List<RsClient>();
		
		public DateTimeOffset DateAdded { get; set; }
		
		public DateTimeOffset LastUpdated { get; set; }
	}

	public class RsClient
	{
		public Game Game { get; set; }
		public string RsUsername { get; set; }
		public string RsPassword { get; set; }
		public int World { get; set; }
		public RsClientProxy Proxy { get; set; }
		public RsClientScript Script { get; set; }
		public RsClientConfig Config { get; set; }
	}

	public class RsClientConfig
	{
		public bool LowCpuMode { get; set; }
		public bool SuperLowCpuMode { get; set; }
		public int EngineTickDelay { get; set; }
		public bool DisableModelRendering { get; set; }
		public bool DisableSceneRendering { get; set; }
	}

	public class RsClientProxy
	{
		public string ProxyId { get; set; }
		public DateTimeOffset Date { get; set; }
		
		public string UserId { get; set; }
		public string Name { get; set; }
		public string Ip { get; set; }
		public string Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}

	public class RsClientScript
	{
		public string ScriptArgs { get; set; }

		public string Name { get; set; }

		public string ScriptId { get; set; }

		public bool IsRepoScript { get; set; }
	}

	public class LauncherLinkKey
	{
		public Guid Key { get; set; }
	}
}