namespace FakeObsidian.Domain.Entities
{
    public class Notification
    {
        public string Id = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public string Content { get; set; }
    }
}
