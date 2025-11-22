using System.ComponentModel.DataAnnotations;

namespace FakeObsidian.Api.Models.User
{
    public class RegRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class LogRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    public class LogoutRequest
    {
        public string? RefreshToken { get; set; }
    }
}
