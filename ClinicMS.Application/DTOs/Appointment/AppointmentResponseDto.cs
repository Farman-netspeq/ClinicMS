using ClinicMS.Domain.Enums;

namespace ClinicMS.Application.DTOs.Appointment
{
    // Full detail for single appointment view
    public class AppointmentResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string AppointmentNumber { get; set; } = string.Empty;

        // Patient info
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientNumber { get; set; } = string.Empty;

        // Doctor info
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;

        // Department info
        public string DepartmentId { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;

        public AppointmentStatus Status { get; set; }
        public string StatusDisplay => Status switch
        {
            AppointmentStatus.Scheduled => "Scheduled",
            AppointmentStatus.CheckedIn => "Checked In",
            AppointmentStatus.Completed => "Completed",
            AppointmentStatus.Cancelled => "Cancelled",
            AppointmentStatus.NoShow => "No Show",
            _ => "Unknown"
        };

        public string ChiefComplaint { get; set; } = string.Empty;
        public string? CancelReason { get; set; }
        public decimal ConsultationFee { get; set; }
        public DateTime TransDate { get; set; }
    }
}