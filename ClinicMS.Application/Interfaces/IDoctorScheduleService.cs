using ClinicMS.Application.DTOs.DoctorSchedule;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.Interfaces
{
    public interface IDoctorScheduleService
    {
        Task<Result<List<DoctorScheduleListItemDto>>> GetByDoctorAsync(string doctorId);
        Task<Result<DoctorScheduleResponseDto>> GetByIdAsync(string id);
        Task<Result<string>> SaveAsync(DoctorScheduleRequestDto dto);
        Task<Result> DeleteAsync(string id);
    }
}
