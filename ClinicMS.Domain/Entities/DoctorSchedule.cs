using System;
using System.Collections.Generic;

namespace ClinicMS.Domain.Entities;

public partial class DoctorSchedule
{
    public string Id { get; set; } = null!;

    public string DoctorId { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public int SlotDurationMinutes { get; set; }

    public bool IsActive { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;
}
