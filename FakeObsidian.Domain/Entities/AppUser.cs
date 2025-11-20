using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace FakeObsidian.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}
