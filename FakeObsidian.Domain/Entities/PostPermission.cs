namespace FakeObsidian.Domain.Entities
{
    public class PostPermission
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PostId { get; set; }
        public Post Post { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public PermissionType Permission { get; set; }

        public string? GrantedById { get; set; }
        public AppUser? GrantedBy { get; set; }

        public DateTime GrantedAt { get; set; } = DateTime.Now;
    }
}
