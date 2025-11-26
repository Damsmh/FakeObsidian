using Microsoft.AspNetCore.Identity;

namespace FakeObsidian.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string Avatar { get; set; } = "static/avatars/default.png";
        public string FullName { get; set; }
        public int Reputation {  get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<AppUser> Friends { get; set; }
        public ICollection<Post> OwnedPosts { get; set; }
        public ICollection<PostPermission> PostPermissions { get; set; }
        public ICollection<PostPermission> GrantedPermissions { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}
