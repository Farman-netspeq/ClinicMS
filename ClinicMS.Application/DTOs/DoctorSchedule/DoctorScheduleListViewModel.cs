using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicMS.Application.DTOs.DoctorSchedule
{
    public class DoctorScheduleListViewModel
    {
        public string DoctorId { get; set; } = default;
        public bool CanEdit { get; set; }
        public List<DoctorScheduleListItemDto> Items { get; set; } = new();
    }
}
