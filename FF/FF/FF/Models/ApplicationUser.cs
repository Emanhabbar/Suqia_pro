using Microsoft.AspNetCore.Identity;

namespace FF.Models
{
    public class ApplicationUser : IdentityUser
    {
       
        public string FullName { get; set; } = string.Empty; // تم تهيئة FullName هنا
        public string DeviceToken { get; set; }

    }
}
