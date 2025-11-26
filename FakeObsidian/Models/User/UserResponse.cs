namespace FakeObsidian.Api.Models.User
{
    public class UserResponse
    {
        public string Id { get; set; }
        public string Avatar { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }

    public class LimitedUserResponse
    {
        public string Avatar { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Friends { get; set; }
    }
}
