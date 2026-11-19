using System;
using System.Collections.Generic;
using System.Numerics;

namespace ClinicMS.Domain.Entities;

public partial class utblCMSDepartments
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<utblCMSDoctors> Doctors { get; set; } = new List<utblCMSDoctors>();
}