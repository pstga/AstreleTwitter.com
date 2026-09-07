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

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Following> Followings { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }
        public DbSet<GroupLike> GroupLikes { get; set; }
        public DbSet<GroupComment> GroupComments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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

            builder.Entity<GroupMessage>()
                    .HasOne(m => m.Group)
                    .WithMany(g => g.Messages)
                    .HasForeignKey(m => m.GroupId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GroupLike>()
                    .HasOne(l => l.User)
                    .WithMany()
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GroupLike>()
        .HasOne(l => l.GroupMessage)
        .WithMany(m => m.GroupLikes)
        .HasForeignKey(l => l.GroupMessageId)
        .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupComment>()
                    .HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict); 

            builder.Entity<GroupComment>()
                .HasOne(c => c.GroupMessage)
                .WithMany(m => m.GroupComments)
                .HasForeignKey(c => c.GroupMessageId)
                .OnDelete(DeleteBehavior.Cascade);

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

            builder.Entity<Post>()
                .HasOne(p => p.OriginalPost)
                .WithMany(p => p.Reposts)
                .HasForeignKey(p => p.OriginalPostId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}