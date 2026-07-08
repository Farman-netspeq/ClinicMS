using ClinicMS.Domain.Enums;

namespace ClinicMS.Application.DTOs.Appointment
{
    // Search + filter params from grid form
    public class AppointmentFilterDto
    {
        // Date range filter
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Filter by specific doctor
        public string? DoctorId { get; set; }

        // Filter by department
        public string? DepartmentId { get; set; }

        // Filter by status (Scheduled/CheckedIn/Completed etc)
        public AppointmentStatus? Status { get; set; }

        // Text search — patient name or appointment number
        public string? SearchTerm { get; set; }

        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}