using System;
using System.Collections.Generic;

namespace ClinicMS.Domain.Entities;

public partial class Doctor
{
    public string Id { get; set; } = null!;

    public string ApplicationUserId { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string DepartmentId { get; set; } = null!;

    public string Specialization { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public decimal ConsultationFee { get; set; }

    public bool IsActive { get; set; }

    public virtual ApplicationUser ApplicationUser { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;

    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
}
