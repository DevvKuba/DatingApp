using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppRole : IdentityRole<int>
    {
        // join table
        public ICollection<AppUserRole> UserRoles { get; set; } = [];
    }
}
