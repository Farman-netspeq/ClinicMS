namespace ClinicMS.Application.DTOs.Appointment
{
    public class CancelAppointmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string CancelReason { get; set; } = string.Empty;
    }
}