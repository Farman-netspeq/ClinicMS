using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicMS.Application.DTOs.DoctorSchedule
{
    public class DoctorScheduleListItemDto
    {
        public string Id { get; set; }=string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int SlotDurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }
}
