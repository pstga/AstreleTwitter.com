using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace AstreleTwitter.com.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePicture { get; set; }
        public bool AccountPrivacy { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Group> CreatedGroups { get; set; }
        public virtual ICollection<UserGroup> UserGroups { get; set; }

        [InverseProperty("Follower")]
        public virtual ICollection<Following> Followings { get; set; }

        [InverseProperty("FollowingUser")]
        public virtual ICollection<Following> Followers { get; set; }
    }
}