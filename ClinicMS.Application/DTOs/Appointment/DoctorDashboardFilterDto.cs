namespace ClinicMS.Application.DTOs.Appointment
{
    public class DoctorDashboardFilterDto
    {
        public string DoctorId { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
    }
}