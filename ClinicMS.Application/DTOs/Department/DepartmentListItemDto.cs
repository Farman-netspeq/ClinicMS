namespace ClinicMS.Application.DTOs.Department
{

    public class DepartmentListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Description shown truncated in grid
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        // Shows "Active" or "Inactive" badge in grid
        public string StatusDisplay => IsActive ? "Active" : "Inactive";
    }
}