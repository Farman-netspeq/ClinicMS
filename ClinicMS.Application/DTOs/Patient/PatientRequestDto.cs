using ClinicMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ClinicMS.Application.DTOs.Patient
{
    public class PatientRequestDto
    {
        public string? Id { get; set; } 

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public BloodGroup BloodGroup { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }
    }
}