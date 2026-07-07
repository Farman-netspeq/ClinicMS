using ClinicMS.Domain.Enums;

namespace ClinicMS.Domain.Entities
{
    public class Patient
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string PatientNumber { get; set; } = string.Empty;
        // auto-generated server-side, e.g. "PAT-000123" 

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public BloodGroup BloodGroup { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;
        // soft-delete flag — BR7

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string? CreatedById { get; set; }  // FK → AspNetUsers.Id, 
    }
}