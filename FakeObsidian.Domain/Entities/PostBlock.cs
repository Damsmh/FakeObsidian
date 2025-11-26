namespace FakeObsidian.Domain.Entities
{
    public class PostBlock
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PostId { get; set; } 
        public Post Post { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
