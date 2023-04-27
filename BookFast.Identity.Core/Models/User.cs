using Microsoft.AspNetCore.Identity;

namespace BookFast.Identity.Core.Models
{
    public class User : IdentityUser
    {
        public string TenantId { get; set; }
    }
}
