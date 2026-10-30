using ClinicMS.Application.DTOs.Department;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.DTOs.Doctor
{
    public class DoctorListPageDto
    {
        public PaginatedResult<DoctorListItemDto> Doctors { get; set; } = new();
        public List<DepartmentListItemDto> Departments { get; set; } = new();
    }
}