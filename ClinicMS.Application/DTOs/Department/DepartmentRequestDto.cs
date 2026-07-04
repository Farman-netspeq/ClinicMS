using System.ComponentModel.DataAnnotations;

namespace ClinicMS.Application.DTOs.Department
{
    public class DepartmentRequestDto
    {
        // Null for create, has value for edit
        public string? Id { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        // IsActive defaults true on create
        public bool IsActive { get; set; } = true;
    }
}