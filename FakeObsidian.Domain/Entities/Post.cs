namespace FakeObsidian.Domain.Entities
{
    public class Post
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public string Image { get; set; } = "static/images/default.png";
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public string OwnerId { get; set; }
        public AppUser Owner { get; set; }

        public ICollection<PostBlock> Blocks { get; set; }
        public ICollection<PostPermission> Permissions { get; set; }
    }
}
