using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AstreleTwitter.com.Models;

namespace AstreleTwitter.com.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DEFINIREA TABELELOR ---
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Following> Followings { get; set; }

        // --- CONFIGURAREA RELAȚIILOR ---
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // IMPORTANT: Păstrează configurările de Identity (Login/Register)
            base.OnModelCreating(builder);

            // 1. UserGroups (Cheie compusă)
            builder.Entity<UserGroup>()
                .HasKey(ug => new { ug.UserId, ug.GroupId });

            builder.Entity<UserGroup>()
                .HasOne(ug => ug.User)
                .WithMany(u => u.UserGroups)
                .HasForeignKey(ug => ug.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserGroup>()
                .HasOne(ug => ug.Group)
                .WithMany(g => g.UserGroups)
                .HasForeignKey(ug => ug.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Followings (Cheie compusă)
            builder.Entity<Following>()
                .HasKey(f => new { f.FollowerId, f.FollowingId });

            builder.Entity<Following>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Followings)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Following>()
                .HasOne(f => f.FollowingUser)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Comments & Likes (Evitare ștergere în cascadă)
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Reposts (Self-Referencing)
            builder.Entity<Post>()
                .HasOne(p => p.OriginalPost)
                .WithMany(p => p.Reposts)
                .HasForeignKey(p => p.OriginalPostId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}