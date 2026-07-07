using ClinicMS.Domain.Enums;

public class PatientResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string PatientNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodGroup BloodGroup { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public int Age => DateTime.Today.Year - DateOfBirth.Year -
        (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
}