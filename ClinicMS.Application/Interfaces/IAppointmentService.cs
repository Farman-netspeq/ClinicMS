using ClinicMS.Application.DTOs.Appointment;
using ClinicMS.Shared.Common;

namespace ClinicMS.Application.Interfaces
{
    public interface IAppointmentService
    {
        // Grid list with filters + paging
        Task<Result<PaginatedResult<AppointmentListItemDto>>>
            GetAppointmentsAsync(AppointmentFilterDto filter);

        // Single appointment detail
        Task<Result<AppointmentResponseDto>>
            GetAppointmentByIdAsync(string id);

        // Book new appointment
        // Enforces BR1 (auto number) + BR2 (no overlap) + BR3 (in schedule)
        Task<Result<string>>
            CreateAppointmentAsync(
                AppointmentRequestDto dto,
                string createdByUserId);
        // createdByUserId = logged-in user's Id

        // Get available slots for Doctor + Date
        Task<Result<List<AppointmentSlotDto>>>
            GetAvailableSlotsAsync(
                string doctorId,
                DateTime date);

        // Cancel appointment — requires reason (BR8)
        Task<Result>
             CancelAppointmentAsync(
                 string id,
                 string cancelReason,
                 string userId);

        // BR4: Scheduled/CheckedIn → Completed. Only assigned doctor or Admin.
        Task<Result> CompleteAppointmentAsync(string id, string currentUserId, bool isAdmin);

        // BR4: Scheduled → CheckedIn. Admin/Receptionist.
        Task<Result> CheckInAppointmentAsync(string id, string userId);

        // BR4: Scheduled → NoShow. Admin/Receptionist.
        Task<Result> MarkNoShowAsync(string id, string userId);

        // BR10: doctor's own appointments only — resolves ApplicationUserId → Doctor.Id internally
        Task<Result<List<AppointmentListItemDto>>> GetDoctorAppointmentsAsync(string doctorUserId, DateTime? date);
    }
}