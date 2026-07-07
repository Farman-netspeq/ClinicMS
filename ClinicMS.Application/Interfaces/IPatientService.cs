using ClinicMS.Application.DTOs.Patient;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.Interfaces
{
    public interface IPatientService
    {
        Task<Result<PaginatedResult<PatientListItemDto>>> GetPatientsAsync(PatientFilterDto filter);
        Task<Result<PatientResponseDto>> GetPatientByIdAsync(string id);
        Task<Result<string>> CreatePatientAsync(PatientRequestDto dto);
        Task<Result> UpdatePatientAsync(PatientRequestDto dto);
        Task<Result> DeactivatePatientAsync(string id);
        Task<Result> ReactivatePatientAsync(string id);
    }
}