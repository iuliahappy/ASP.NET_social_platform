using SocialPlatformTime.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace SocialPlatformTime.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<FollowRequest> FollowRequests { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Group> Groups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // definirea relatiei many-to-many dintre Article si Bookmark

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<Message>()
                .HasKey(ab => new { ab.Id, ab.UserId, ab.ConversationId });


            // definire relatii cu modelele ApplicationUser si Conversation (FK)

            modelBuilder.Entity<Message>()
                .HasOne(ab => ab.User)
                .WithMany(ab => ab.Messages)
                .HasForeignKey(ab => ab.UserId);

            modelBuilder.Entity<Message>()
                .HasOne(ab => ab.Conversation)
                .WithMany(ab => ab.Messages)
                .HasForeignKey(ab => ab.ConversationId);


            // definire primary key compus
            modelBuilder.Entity<Role>()
                .HasKey(bb => new { bb.Id, bb.UserId, bb.GroupId });


            // definire relatii cu modelele ApplicationUser si Group (FK)

            modelBuilder.Entity<Role>()
                .HasOne(bb => bb.User)
                .WithMany(bb => bb.Roles)
                .HasForeignKey(bb => bb.UserId);

            modelBuilder.Entity<Role>()
                .HasOne(bb => bb.Group)
                .WithMany(bb => bb.Roles)
                .HasForeignKey(bb => bb.GroupId);



        }
    }
}
