using ClinicMS.Domain.Enums;

namespace ClinicMS.Domain.Entities
{
    public class Appointment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Auto-generated server-side
        public string AppointmentNumber { get; set; } = string.Empty;

        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DepartmentId { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public AppointmentStatus Status { get; set; }
            = AppointmentStatus.Scheduled;

        public string ChiefComplaint { get; set; } = string.Empty;

        public string? CancelReason { get; set; }
        public DateTime TransDate { get; set; } = DateTime.UtcNow;
        public string LastUpdatedBy { get; set; } = string.Empty;
        // ── Navigation properties ──────────────────────────
        // EF fills these with .Include() — not DB columns
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public Department? Department { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
    }
}