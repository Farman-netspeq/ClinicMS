using ClinicMS.Application.DTOs.Department;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.Interfaces
{

    public interface IDepartmentService
    {
        Task<Result<PaginatedResult<DepartmentListItemDto>>> GetDepartmentsAsync(
            DepartmentFilterDto filter);

        Task<Result<DepartmentResponseDto>> GetDepartmentByIdAsync(string id);

        // CREATE new department
        Task<Result<string>> CreateDepartmentAsync(
            DepartmentRequestDto dto);

        Task<Result> UpdateDepartmentAsync(DepartmentRequestDto dto);

        // DEACTIVATE (soft disable) — not hard delete
        Task<Result> DeactivateDepartmentAsync(string id);

        // GET all active departments as simple list
        Task<Result<List<DepartmentListItemDto>>> GetActiveDepartmentsAsync();
    }
}