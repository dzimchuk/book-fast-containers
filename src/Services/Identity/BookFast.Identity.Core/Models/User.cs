using Microsoft.AspNetCore.Identity;

namespace BookFast.Identity.Core.Models
{
    public class User : IdentityUser<string>
    {
        public string TenantId { get; set; }
    }
}
