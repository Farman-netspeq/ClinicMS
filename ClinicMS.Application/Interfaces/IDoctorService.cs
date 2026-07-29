using ClinicMS.Application.DTOs.Doctor;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<Result<PaginatedResult<DoctorListItemDto>>> GetDoctorsAsync(
            DoctorFilterDto filter);

        Task<Result<DoctorResponseDto>> GetDoctorByIdAsync(string id);

        // Create = also creates ApplicationUser + assigns Doctor role
        Task<Result<string>> CreateDoctorAsync(DoctorRequestDto dto);

        // Update = updates Doctor entity only
        // Does NOT change login email/password
        Task<Result> UpdateDoctorAsync(DoctorRequestDto dto);

        Task<Result> DeactivateDoctorAsync(string id);
        Task<Result> ReactivateDoctorAsync(string id);

        // For cascading dropdown:
        // Department selected → load doctors in that dept
        Task<Result<List<DoctorListItemDto>>> GetDoctorsByDepartmentAsync(
            string departmentId);
        Task<Result<string>> GetDoctorIdByUserIdAsync(string userId);
    }
}