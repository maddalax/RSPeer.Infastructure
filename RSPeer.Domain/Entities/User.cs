using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using RSPeer.Domain.Constants;

namespace RSPeer.Domain.Entities
{
	public class User
	{
		private List<Group> _groups;
		public int Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public int Balance { get; set; }
		public bool IsEmailVerified { get; set; }
		
		[NotMapped]
		public int Instances { get; set; }
		
		public Guid? LinkKey { get; set; }
		
		public Guid LegacyId { get; set; }
		
		public bool Disabled { get; set; }

		public bool HasGroup(string name)
		{
			return !Disabled && Groups != null && Groups.Any(w => w?.Name != null && string.Equals(w.Name, name, StringComparison.InvariantCultureIgnoreCase));
		}

		public bool IsOwner => HasGroup(GroupConstants.Owners);
		
		public bool IsModerator => HasGroup(GroupConstants.Moderator);
		
		
		[JsonIgnore] 
		public ICollection<UserGroup> UserGroups { get; } = new List<UserGroup>();

		[NotMapped]
		public List<Group> Groups
		{
			get => _groups ??= UserGroups.Select(w => w.Group).ToList();
			set => _groups = value;
		}
		
		public DiscordAccount DiscordAccount { get; set; }

		public HashSet<string> GroupNames => (Groups ?? new List<Group>()).Where(w => w != null).Select(w => w.Name).ToHashSet();
	}
}