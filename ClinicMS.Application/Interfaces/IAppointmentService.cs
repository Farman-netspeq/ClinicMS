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
        // Web controller reads from cookie claims → passes here

        // Get available slots for Doctor + Date
        // Used by cascading dropdown in booking form
        // Returns list of SlotDto (StartTime, EndTime, Display)
        Task<Result<List<AppointmentSlotDto>>>
            GetAvailableSlotsAsync(
                string doctorId,
                DateTime date);

        // Cancel appointment — requires reason (BR8)
        Task<Result>
            CancelAppointmentAsync(
                string id,
                string cancelReason);
    }
}