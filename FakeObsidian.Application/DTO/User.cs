namespace FakeObsidian.Application.DTO
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Avatar { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }
}
