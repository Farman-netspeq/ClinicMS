namespace ClinicMS.Domain.Entities
{
    // Maps to utblCMSDoctorSchedules table
    // Defines WHEN a doctor is available each week

    public class DoctorSchedule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DoctorId { get; set; } = string.Empty;
        public Doctor? Doctor { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}