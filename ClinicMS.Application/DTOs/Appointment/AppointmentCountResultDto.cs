public class AppointmentCountResult { public int TotalCount { get; set; } }

public class AppointmentScheduleResultDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
}