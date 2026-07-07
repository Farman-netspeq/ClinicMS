namespace ClinicMS.Application.DTOs.Doctor
{
    public class DoctorFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? DepartmentId { get; set; }
        // Filter by department dropdown on grid
        public bool? IsActive { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}