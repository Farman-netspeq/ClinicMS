namespace ClinicMS.Application.DTOs.Department
{

    public class DepartmentFilterDto
    {
        public string? SearchTerm { get; set; }

        public bool? IsActive { get; set; }

        public int PageNo { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}