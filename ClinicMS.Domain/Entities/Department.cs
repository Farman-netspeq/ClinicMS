using System;
using System.Collections.Generic;
using System.Numerics;

namespace ClinicMS.Domain.Entities;

public partial class Department
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}