namespace ClinicMS.Application.DTOs.Appointment
{
    // Represents one available time slot
    // Used in booking form cascading dropdown:
    public class AppointmentSlotDto
    {
        public string StartTime { get; set; } = string.Empty;

        public string EndTime { get; set; } = string.Empty;

        // "09:00 - 09:30" — shown in dropdown
        public string Display => $"{StartTime} - {EndTime}";

        // CustomAjax cascading dropdown needs Value + Text:
        // Value = what gets submitted with form
        // Text = what user sees in dropdown
        public string Value => StartTime;
        public string Text => Display;
    }
}