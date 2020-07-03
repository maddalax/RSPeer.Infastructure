using System;
using System.Collections.Generic;
using System.Linq;
using RSPeer.Domain.Entities;

namespace RSPeer.Domain.Dtos
{
	public class ScriptDto
	{
		public ScriptDto(Script script)
		{
			if (script == null)
				return;
			Id = script.Id;
			DoesUserOwn = false;
			Name = script.Name;
			Version = script.Version;
			Author = script.User?.Username;
			AuthorId = script.UserId;
			Description = script.Description;
			Category = script.Category;
			CategoryFormatted = Category.ToString();
			LastUpdate = script.LastUpdate;
			TotalUsers = script.TotalUsers;
			Price = script.Price;
			PriceFormatted = Price == 0 ? "Free" : $"${Price:#.##}";
			Type = script.Type;
			ForumThread = !string.IsNullOrEmpty(script.DiscourseThread) ? $"https://discourse.rspeer.org/t/{script.DiscourseThread}" : script.ForumThread;
			TypeFormatted = Type.ToString();
			Instances = script.Instances;
			Status = script.Status;
			StatusFormatted = script.Status.ToString();
			RepositoryUrl = script.RepositoryUrl;
			PrivateScriptAccesses = script.PrivateScriptAccesses?.Select(a => new PrivateScriptAccessDto(a));
			Game = script.Game;
			Disabled = script.Disabled;
		}

		public ScriptDto()
		{

		}

		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Version { get; set; }
		public string Author { get; set; }
		public int AuthorId { get; set; }
		public string Description { get; set; }
		public ScriptCategory Category { get; set; }
		public string CategoryFormatted { get; set; }
		public DateTimeOffset LastUpdate { get; set; }
		public ulong FileSize { get; set; }
		public long TotalUsers { get; set; }
		public decimal? Price { get; set; }
		public string PriceFormatted { get; set; }
		public string TypeFormatted { get; set; }
		public ScriptType Type { get; set; }
		public bool DoesUserOwn { get; set; }
		public int? Instances { get; set; }
		public string ForumThread { get; set; }
		public ScriptStatus Status { get; set; }
		public string StatusFormatted { get; set; }

		public string RepositoryUrl { get; set; }

		public Game Game { get; set; }

		public string GameFormatted => Game.ToString().ToUpper();

		public IEnumerable<PrivateScriptAccessDto> PrivateScriptAccesses { get; set; }

		public bool Disabled { get; set; }
	}
}