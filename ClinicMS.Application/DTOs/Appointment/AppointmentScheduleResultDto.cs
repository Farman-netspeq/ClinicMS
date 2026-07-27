namespace ClinicMS.Application.DTOs.Appointment
{
    public class AppointmentScheduleResultDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }
    }
}