using Microsoft.AspNetCore.Identity;

namespace ISMSponsor.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? SponsorId { get; set; }
        public bool IsActive { get; set; } = true;

        // navigation
        // sponsor relationship will be loaded when necessary
    }
}