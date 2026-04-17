using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Viora.Models;

namespace Viora.Data
{
    public class VioraDBContext:IdentityDbContext<ApplicationUser,IdentityRole<int>, int>
    {
        public VioraDBContext(DbContextOptions<VioraDBContext> options) : base(options) { }


        public DbSet<Chat> Chats { get; set; }

        public DbSet<ChatSummary> ChatSummaries { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<UserFile> UserFiles { get; set; }

        public DbSet<HelpPost> HelpPosts { get; set; }

        public DbSet<HelpComment> HelpComments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // ChatMessage → User
            builder.Entity<ChatMessage>()
                .HasOne(m => m.User)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // HelpComment → User
            builder.Entity<HelpComment>()
                .HasOne(c => c.User)
                .WithMany(u => u.HelpComments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // HelpPost → User
            builder.Entity<HelpPost>()
                .HasOne(p => p.User)
                .WithMany(u => u.HelpPosts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // File → User
            builder.Entity<UserFile>()
                .HasOne(f => f.User)
                .WithMany(u => u.Files)
                .HasForeignKey(f => f.UserOwnerId)
                .OnDelete(DeleteBehavior.NoAction);
        }



    }
}
