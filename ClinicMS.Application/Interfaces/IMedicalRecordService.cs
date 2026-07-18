using ClinicMS.Application.DTOs.MedicalRecord;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<Result<MedicalRecordResponseDto>> GetByAppointmentIdAsync(string appointmentId);
        Task<Result<string>> CreateAsync(MedicalRecordRequestDto dto, string doctorUserId, bool isAdmin);
    }
}