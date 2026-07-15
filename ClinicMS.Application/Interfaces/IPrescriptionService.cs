using ClinicMS.Application.DTOs.Prescription;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.Interfaces
{
    public interface IPrescriptionService
    {
        Task<Result<PrescriptionResponseDto>> GetByAppointmentIdAsync(string appointmentId);
        Task<Result<string>> CreateAsync(PrescriptionRequestDto dto, string doctorUserId, bool isAdmin);
    }
}