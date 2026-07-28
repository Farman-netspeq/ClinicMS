using ClinicMS.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ClinicMS.Domain.Entities;

public partial class Patient
{
    public string Id { get; set; } = null!;

    public string PatientNumber { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public Gender Gender { get; set; }

    public BloodGroup BloodGroup { get; set; }

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }
    public DateTime TransDate { get; set; }

    public string? LastUpdatedBy { get; set; }
}
