using ClinicMS.Domain.Enums;

namespace ClinicMS.Application.DTOs.Appointment
{
    // One row in appointments grid
    public class AppointmentListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string AppointmentNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientNumber { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; }

        // Badge color for status in grid
        public string StatusDisplay => Status switch
        {
            AppointmentStatus.Scheduled => "Scheduled",
            AppointmentStatus.CheckedIn => "Checked In",
            AppointmentStatus.Completed => "Completed",
            AppointmentStatus.Cancelled => "Cancelled",
            AppointmentStatus.NoShow => "No Show",
            _ => "Unknown"
        };

        // CSS badge class based on status
        // Used in _List.cshtml for colored badges
        public string StatusBadgeClass => Status switch
        {
            AppointmentStatus.Scheduled => "badge-primary",
            AppointmentStatus.CheckedIn => "badge-info",
            AppointmentStatus.Completed => "badge-success",
            AppointmentStatus.Cancelled => "badge-danger",
            AppointmentStatus.NoShow => "badge-warning",
            _ => "badge-secondary"
        };

        public string ChiefComplaint { get; set; } = string.Empty;
    }
}