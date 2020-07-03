using System.Linq;
using Microsoft.EntityFrameworkCore;
using RSPeer.Domain.Entities.Discourse;

namespace RSPeer.Persistence
{
    public class DiscourseContext : DbContext
    {
        public DiscourseContext(DbContextOptions options) : base(options)
        {
        }
        
        public DbSet<DiscourseTopic> Topics { get; set; }
        
        public DbSet<DiscourseUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Model.GetEntityTypes().ToList()
                .ForEach(e => e.SetTableName(e.GetTableName().ToLower()));

            builder.Model.GetEntityTypes()
                .SelectMany(e => e.GetProperties())
                .ToList()
                .ForEach(c => c.SetColumnName(c.GetColumnName().ToLower()));
            
            base.OnModelCreating(builder);
        }
    }
}