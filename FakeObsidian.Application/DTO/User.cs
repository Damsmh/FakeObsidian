namespace FakeObsidian.Application.DTO
{
    public class UserRolesDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class UpdateRolesDto
    {
        public List<string> Roles { get; set; }
    }

}
