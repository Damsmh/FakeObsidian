using System;

namespace FakeObsidian.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }

        public string UserId { get; set; } = null!;
        public AppUser? User { get; set; }
    }
}