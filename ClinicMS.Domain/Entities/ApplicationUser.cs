using Microsoft.AspNetCore.Identity;

namespace ClinicMS.Domain.Entities
{
    // IdentityUser already has: Id, Email, UserName, PasswordHash, PhoneNumber etc.
    // We extend it to add our own fields.
    public class ApplicationUser : IdentityUser
    {
        // Full name of the person logging in (Admin/Receptionist/Doctor)
       
        public string FullName { get; set; } = string.Empty;

        // IsActive — soft disable a user without deleting them
        public bool IsActive { get; set; } = true;

        // When was this account created
        public DateTime TransDate { get; set; } = DateTime.UtcNow;
    }
}