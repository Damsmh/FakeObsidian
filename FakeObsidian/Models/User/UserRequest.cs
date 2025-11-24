using System.ComponentModel.DataAnnotations;

namespace FakeObsidian.Api.Models.User
{
    public class UpdateUserRequest
    {
        [Required]
        public string Avatar { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public List<string> Roles { get; set; }
    }

    public class AddFriendRequest
    {
        [Required]
        public string Id { get; set; }
    }

    public class RemoveFriendRequest
    {
        [Required]
        public string Id { get; set; }
    }
}
