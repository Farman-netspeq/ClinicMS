namespace ClinicMS.Application.DTOs.Department
{

    public class DepartmentResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // How many doctors in this department
        public int DoctorCount { get; set; }
    }
}