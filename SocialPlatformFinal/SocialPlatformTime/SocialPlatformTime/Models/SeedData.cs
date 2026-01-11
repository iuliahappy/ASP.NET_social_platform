using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

namespace SocialPlatformTime.Models
{
    // PASUL 4: useri si roluri
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // 1. Verificăm și creăm Rolurile și Userii de bază (Logica originală)
                if (!context.Roles.Any())
                {
                    context.Roles.AddRange(
                        new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Administrator", NormalizedName = "ADMINISTRATOR" },
                        new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7212", Name = "Registered_User", NormalizedName = "REGISTERED_USER" }
                    );

                    var hasher = new PasswordHasher<ApplicationUser>();

                    context.Users.AddRange(
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                            UserName = "admin@test.com",
                            EmailConfirmed = true,
                            NormalizedEmail = "ADMIN@TEST.COM",
                            Email = "admin@test.com",
                            NormalizedUserName = "ADMIN@TEST.COM",
                            PasswordHash = hasher.HashPassword(null, "Admin1!")
                        },
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                            UserName = "user@test.com",
                            EmailConfirmed = true,
                            NormalizedEmail = "USER@TEST.COM",
                            Email = "user@test.com",
                            NormalizedUserName = "USER@TEST.COM",
                            PasswordHash = hasher.HashPassword(null, "User1!")
                        },
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb3",
                            UserName = "member@test.com",
                            EmailConfirmed = true,
                            NormalizedEmail = "MEMBER@TEST.COM",
                            Email = "member@test.com",
                            NormalizedUserName = "MEMBER@TEST.COM",
                            PasswordHash = hasher.HashPassword(null, "Member1!")
                        }
                    );

                    context.UserRoles.AddRange(
                        new IdentityUserRole<string> { RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210", UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0" },
                        new IdentityUserRole<string> { RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212", UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2" },
                        new IdentityUserRole<string> { RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212", UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3" }
                    );
                    context.SaveChanges();
                }

                // 2. CREAREA GRUPURILOR DOAR DACĂ NU EXISTĂ
                if (!context.Groups.Any())
                {
                    // Definire Grupuri
                    var group1 = new Group { Name = "C# Enthusiasts", Description = "Everything about .NET", CreateDGroup = DateTime.Now };
                    var group2 = new Group { Name = "Web Design", Description = "Frontend and UI/UX", CreateDGroup = DateTime.Now };
                    var group3 = new Group { Name = "General Chat", Description = "Random discussions", CreateDGroup = DateTime.Now };

                    // Definire Conversații
                    var conv1 = new Conversation { Group = group1 };
                    var conv2 = new Conversation { Group = group2 };
                    var conv3 = new Conversation { Group = group3 };

                    context.Conversations.AddRange(conv1, conv2, conv3);
                    context.Groups.AddRange(group1, group2, group3);

                    context.SaveChanges(); // Generăm ID-urile

                    // --- ASOCIERE ROLURI (RoleTable) ---
                    context.GroupRoles.AddRange(
                        // Owneri (unul per grup)
                        new RoleTable { GroupId = group1.Id, ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb0", RoleName = "Owner" },
                        new RoleTable { GroupId = group2.Id, ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", RoleName = "Owner" },
                        new RoleTable { GroupId = group3.Id, ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb3", RoleName = "Owner" },

                        // Membri suplimentari (pentru a avea pe cine da Kick sau a vedea în listă)
                        new RoleTable { GroupId = group1.Id, ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb3", RoleName = "Member" },
                        new RoleTable { GroupId = group2.Id, ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb0", RoleName = "Member" },
                        new RoleTable { GroupId = group3.Id, ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", RoleName = "Member" }
                    );

                    // --- ACCES LA CHAT (UserConversation) ---
                    // Trebuie să fie aceiași useri ca în RoleTable
                    context.UserConversations.AddRange(
                        // Grupul 1
                        new UserConversation { ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb0", ConversationId = conv1.Id, LastEntry = DateTime.Now },
                        new UserConversation { ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb3", ConversationId = conv1.Id, LastEntry = DateTime.Now },

                        // Grupul 2
                        new UserConversation { ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", ConversationId = conv2.Id, LastEntry = DateTime.Now },
                        new UserConversation { ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb0", ConversationId = conv2.Id, LastEntry = DateTime.Now },

                        // Grupul 3
                        new UserConversation { ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb3", ConversationId = conv3.Id, LastEntry = DateTime.Now },
                        new UserConversation { ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb2", ConversationId = conv3.Id, LastEntry = DateTime.Now }
                    );

                    context.SaveChanges();
                }

                    context.Posts.AddRange(
                        new Post { PostDescription = "First Post", TextContent = "Hello world! This is my first post on the platform.", Date = DateTime.Now, ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb2" },
                        new Post { PostDescription = "Tech Tips", TextContent = "Always use SaveChanges() after modifying the context.", Date = DateTime.Now, ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb0" },
                        new Post { PostDescription = "Inspiration", TextContent = "The best way to predict the future is to invent it.", Date = DateTime.Now, ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb3" },
                        new Post { PostDescription = "ASP.NET Core", TextContent = "Learning Dependency Injection is crucial for modern web apps.", Date = DateTime.Now, ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb2" },
                        new Post { PostDescription = "Weekend Vibes", TextContent = "Happy coding this weekend everyone!", Date = DateTime.Now, ApplicationUserId = "8e445865-a24d-4543-a6c6-9443d048cdb3" }
                    );
                    context.SaveChanges();
                }
            }
        }
}

