using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicMS.Application.DTOs.DoctorSchedule
{
    public class DoctorScheduleResponseDto
    {
        public string Id { get; set; }=string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }
}
