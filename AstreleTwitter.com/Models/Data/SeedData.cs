using AstreleTwitter.com.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AstreleTwitter.com.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = serviceProvider.GetRequiredService<ApplicationDbContext>())
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roleNames = { "Admin", "User" };

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                var adminEmail = "admin@test.com";
                var user1Email = "user1@test.com";
                var user2Email = "user2@test.com";

                ApplicationUser adminUser = null;
                ApplicationUser normalUser1 = null;
                ApplicationUser normalUser2 = null;

                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "Principal",
                        EmailConfirmed = true,
                        AccountPrivacy = false,
                        Bio = "Acesta este contul de administrator.",
                        ProfilePicture = "https://ui-avatars.com/api/?name=Admin+Principal&background=0D8ABC&color=fff"
                    };
                    var result = await userManager.CreateAsync(adminUser, "Admin!123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
                else
                {
                    adminUser = await userManager.FindByEmailAsync(adminEmail);
                }

                if (await userManager.FindByEmailAsync(user1Email) == null)
                {
                    normalUser1 = new ApplicationUser
                    {
                        UserName = user1Email,
                        Email = user1Email,
                        FirstName = "Ion",
                        LastName = "Popescu",
                        EmailConfirmed = true,
                        AccountPrivacy = false,
                        Bio = "Utilizator pasionat de IT.",
                        ProfilePicture = "https://ui-avatars.com/api/?name=Ion+Popescu&background=ffb6b9&color=fff"
                    };
                    var result = await userManager.CreateAsync(normalUser1, "User!123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(normalUser1, "User");
                    }
                }
                else
                {
                    normalUser1 = await userManager.FindByEmailAsync(user1Email);
                }

                if (await userManager.FindByEmailAsync(user2Email) == null)
                {
                    normalUser2 = new ApplicationUser
                    {
                        UserName = user2Email,
                        Email = user2Email,
                        FirstName = "Maria",
                        LastName = "Ionescu",
                        EmailConfirmed = true,
                        AccountPrivacy = true,
                        Bio = "Cont privat. Îmi place natura.",
                        ProfilePicture = "https://ui-avatars.com/api/?name=Maria+Ionescu&background=28a745&color=fff"
                    };
                    var result = await userManager.CreateAsync(normalUser2, "User!123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(normalUser2, "User");
                    }
                }
                else
                {
                    normalUser2 = await userManager.FindByEmailAsync(user2Email);
                }

                if (!context.Groups.Any())
                {
                    var group1 = new Group { Title = "Tehnologie", Description = "Totul despre gadgeturi.", CreatorId = adminUser.Id, Date = DateTime.Now.AddDays(-10) };
                    var group2 = new Group { Title = "Călătorii", Description = "Locuri frumoase din lume.", CreatorId = normalUser1.Id, Date = DateTime.Now.AddDays(-5) };
                    var group3 = new Group { Title = "Muzică", Description = "Discuții despre albume noi.", CreatorId = normalUser2.Id, Date = DateTime.Now.AddDays(-2) };

                    context.Groups.AddRange(group1, group2, group3);
                    context.SaveChanges();

                    context.UserGroups.AddRange(
                        new UserGroup { UserId = adminUser.Id, GroupId = group1.Id, Status = "Accepted" },
                        new UserGroup { UserId = normalUser1.Id, GroupId = group1.Id, Status = "Accepted" },

                        new UserGroup { UserId = normalUser1.Id, GroupId = group2.Id, Status = "Accepted" },
                        new UserGroup { UserId = adminUser.Id, GroupId = group2.Id, Status = "Accepted" },

                        new UserGroup { UserId = normalUser2.Id, GroupId = group3.Id, Status = "Accepted" },
                        new UserGroup { UserId = normalUser1.Id, GroupId = group3.Id, Status = "Pending" }
                    );
                    context.SaveChanges();
                }

                if (!context.Posts.Any())
                {
                    context.Posts.AddRange(
                        new Post { Content = "Bun venit pe noua platformă! Acesta este un mesaj de la Admin.", UserId = adminUser.Id, Date = DateTime.Now.AddDays(-2) },
                        new Post { Content = "Salut! Eu sunt Ion și testez aplicația.", UserId = normalUser1.Id, Date = DateTime.Now.AddDays(-1) },
                        new Post { Content = "O zi frumoasă tuturor! ☀️", UserId = normalUser2.Id, Date = DateTime.Now.AddHours(-5) },
                        new Post { Content = "Cine merge la conferința de IT săptămâna viitoare?", UserId = adminUser.Id, Date = DateTime.Now.AddHours(-2) },
                        new Post { Content = "Am făcut o poză grozavă azi, dar nu știu cum să o urc încă.", UserId = normalUser1.Id, Date = DateTime.Now.AddMinutes(-30) }
                    );
                    context.SaveChanges();
                }

                if (!context.Followings.Any())
                {
                    context.Followings.AddRange(
                        new Following { FollowerId = adminUser.Id, FollowingId = normalUser1.Id, Status = "Accepted" },
                        new Following { FollowerId = normalUser1.Id, FollowingId = adminUser.Id, Status = "Accepted" },
                        new Following { FollowerId = normalUser1.Id, FollowingId = normalUser2.Id, Status = "Accepted" },
                        new Following { FollowerId = normalUser2.Id, FollowingId = adminUser.Id, Status = "Accepted" }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}