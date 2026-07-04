using System.ComponentModel.DataAnnotations;

namespace ClinicMS.Application.DTOs.Doctor
{
    public class DoctorRequestDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public string DepartmentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialization is required")]
        [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
        public string Specialization { get; set; } = string.Empty;

        [Required(ErrorMessage = "License number is required")]
        [StringLength(50, ErrorMessage = "License cannot exceed 50 characters")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Range(0, 99999, ErrorMessage = "Fee must be 0 or greater")]
        public decimal ConsultationFee { get; set; }

        // Login credentials — only for CREATE
        // On edit → email/password not changed here
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        public bool IsActive { get; set; } = true;
    }
}