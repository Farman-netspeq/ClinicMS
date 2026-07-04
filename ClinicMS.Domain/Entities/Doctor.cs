using ClinicMS.Domain.Enums;

namespace ClinicMS.Domain.Entities
{
    // Maps to utblCMSDoctors table
    // Doctor has TWO identities:
    // 1. ApplicationUser (login account — in AspNetUsers)
    // 2. Doctor entity (medical profile — in utblCMSDoctors)
    // They link via ApplicationUserId FK
    public class Doctor
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // FK → AspNetUsers.Id
        // When doctor logs in, we find their Doctor record using this
        public string ApplicationUserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        // FK → Department.Id
        public string DepartmentId { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;
        // e.g. "Cardiology", "Orthopedics"

        public string LicenseNumber { get; set; } = string.Empty;
        // Unique medical license — enforced in EF config

        public decimal ConsultationFee { get; set; }
        // How much this doctor charges per visit

        public bool IsActive { get; set; } = true;

        // ── Navigation properties ──────────────────────────

        public ApplicationUser? ApplicationUser { get; set; }
        // → linked login account

        public Department? Department { get; set; }
        // → which department this doctor belongs to

        public ICollection<DoctorSchedule> Schedules { get; set; }
            = new List<DoctorSchedule>();
        // → this doctor's weekly availability blocks
    }
}