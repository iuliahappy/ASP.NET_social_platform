using SocialPlatformTime.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<RoleTable> RoleTables { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Group> Groups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // definirea relatiei many-to-many dintre ApplicationUser si Conversation

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


            // definirea relatiei many-to-many dintre ApplicationUser si Group

            // definire primary key compus
            modelBuilder.Entity<RoleTable>()
                .HasKey(bb => new { bb.Id, bb.UserId, bb.GroupId });


            // definire relatii cu modelele ApplicationUser si Group (FK)

            modelBuilder.Entity<RoleTable>()
                .HasOne(bb => bb.User)
                .WithMany(bb => bb.RoleTables)
                .HasForeignKey(bb => bb.UserId);

            modelBuilder.Entity<RoleTable>()
                .HasOne(bb => bb.Group)
                .WithMany(bb => bb.RoleTables)
                .HasForeignKey(bb => bb.GroupId);


            // rezolvare stergere in cascada pentru Follower si Following - vom rezolva logica de stergere in Controller
            // Configurarea pentru FollowRequest
            modelBuilder.Entity<FollowRequest>()
                .HasOne(fr => fr.Follower)
                .WithMany(fr => fr.FollowRequestsSent) 
                .HasForeignKey(fr => fr.FollowerId)
                .OnDelete(DeleteBehavior.Restrict); // Opreste stergerea automata

            modelBuilder.Entity<FollowRequest>()
                .HasOne(fr => fr.Following)
                .WithMany(fr => fr.FollowRequestsReceived) 
                .HasForeignKey(fr => fr.FollowingId)
                .OnDelete(DeleteBehavior.Restrict); // Opreste stergerea automata

            //trebuie sa oprim stergerea automata si pentru Comments pentru ca:
            // daca lasam fara :
            // daca stergem un User, se sterg in cascada comentariile scrise de el
            // daca stergem un User, se sterg in cascada postarile lui, iar postarile au comentarii care se vor sterge in cascada
            //SQL Server vede ca stergerea unui User ajunge la stergerea unui Comentariu pe doua cai diferite si blocheaza actiunea

            // SOLUTIE: oprim stergerea automata pe relatia User -> Comment, la Post o putem lasa, ne vom asigura din Controller ca se sterge totul corect
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments) 
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Opresc stergerea automata User -> Comment

            // se intampla acelsi lucru si cu Reaction (User->Reaction, User->Post->Reaction)
            // rezolvare prin oprirea stergerii in cascada pe relatia User -> Reaction, rezolvam in Controller 
            // CONFIGURARE PENTRU REACTION (Liking)
            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Opresc stergerea automata User -> Reaction
        }
    }
}
