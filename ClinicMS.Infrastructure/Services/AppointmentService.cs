using ClinicMS.Application.DTOs.Appointment;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Domain.Enums;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicMS.Infrastructure.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(ApplicationDbContext context, ILogger<AppointmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── GET PAGED LIST ────────────────────────────────────
        public async Task<Result<PaginatedResult<AppointmentListItemDto>>>
            GetAppointmentsAsync(AppointmentFilterDto filter)
        {
            try
            {
                var searchTermParam = new SqlParameter("@SearchTerm", (object?)filter.SearchTerm ?? DBNull.Value);
                var doctorIdParam = new SqlParameter("@DoctorId", (object?)filter.DoctorId ?? DBNull.Value);
                var departmentIdParam = new SqlParameter("@DepartmentId", (object?)filter.DepartmentId ?? DBNull.Value);
                var statusParam = new SqlParameter("@Status", (object?)(int?)filter.Status ?? DBNull.Value);
                var fromDateParam = new SqlParameter("@FromDate", (object?)filter.FromDate ?? DBNull.Value);
                var toDateParam = new SqlParameter("@ToDate", (object?)filter.ToDate ?? DBNull.Value);
                var pageNoParam = new SqlParameter("@PageNo", filter.PageNo);
                var pageSizeParam = new SqlParameter("@PageSize", filter.PageSize);

                var items = await _context.Set<AppointmentListItemDto>()
                    .FromSqlRaw(
                        "EXEC udspApptPaged @SearchTerm, @DoctorId, @DepartmentId, @Status, @FromDate, @ToDate, @PageNo, @PageSize",
                        searchTermParam, doctorIdParam, departmentIdParam, statusParam, fromDateParam, toDateParam, pageNoParam, pageSizeParam)
                    .ToListAsync();

                var countSearchTermParam = new SqlParameter("@SearchTerm", (object?)filter.SearchTerm ?? DBNull.Value);
                var countDoctorIdParam = new SqlParameter("@DoctorId", (object?)filter.DoctorId ?? DBNull.Value);
                var countDepartmentIdParam = new SqlParameter("@DepartmentId", (object?)filter.DepartmentId ?? DBNull.Value);
                var countStatusParam = new SqlParameter("@Status", (object?)(int?)filter.Status ?? DBNull.Value);
                var countFromDateParam = new SqlParameter("@FromDate", (object?)filter.FromDate ?? DBNull.Value);
                var countToDateParam = new SqlParameter("@ToDate", (object?)filter.ToDate ?? DBNull.Value);

                var countResult = await _context.Set<AppointmentCountResultDto>()
                    .FromSqlRaw(
                        "EXEC udspApptPagedCount @SearchTerm, @DoctorId, @DepartmentId, @Status, @FromDate, @ToDate",
                        countSearchTermParam, countDoctorIdParam, countDepartmentIdParam, countStatusParam, countFromDateParam, countToDateParam)
                    .ToListAsync();

                var totalCount = countResult.FirstOrDefault()?.TotalCount ?? 0;

                return Result<PaginatedResult<AppointmentListItemDto>>.Ok(
                    new PaginatedResult<AppointmentListItemDto>
                    {
                        Items = items,
                        TotalCount = totalCount,
                        Page = filter.PageNo,
                        PageSize = filter.PageSize
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointments");
                return Result<PaginatedResult<AppointmentListItemDto>>.Fail("Failed to fetch appointments");
            }
        }

        // ── GET BY ID ─────────────────────────────────────────
        public async Task<Result<AppointmentResponseDto>>
            GetAppointmentByIdAsync(string id)
        {
            try
            {
                var idParam = new SqlParameter("@Id", id);

                var results = await _context.Set<AppointmentResponseDto>()
                    .FromSqlRaw("EXEC udspApptGetById @Id", idParam)
                    .ToListAsync();

                var dto = results.FirstOrDefault();
                if (dto == null)
                    return Result<AppointmentResponseDto>.Fail("Appointment not found");

                return Result<AppointmentResponseDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointment {Id}", id);
                return Result<AppointmentResponseDto>.Fail("Failed to fetch appointment");
            }
        }

        // ── CREATE APPOINTMENT ────────────────────────────────
        public async Task<Result<string>> CreateAppointmentAsync(
            AppointmentRequestDto dto, string createdByUserId)
        {
            try
            {
                if (!TimeSpan.TryParse(dto.StartTime, out TimeSpan startTime))
                    return Result<string>.Fail("Invalid start time format");

                // BR3 CHECK 1: not in the past
                var appointmentDateTime = dto.AppointmentDate.Date + startTime;
                if (appointmentDateTime < DateTime.Now)
                    return Result<string>.Fail("Cannot book appointment in the past");

                // BR3 CHECK 2: within schedule + slot alignment
                var doctorIdParam = new SqlParameter("@DoctorId", dto.DoctorId);
                var dayOfWeekParam = new SqlParameter("@DayOfWeek", (int)dto.AppointmentDate.DayOfWeek);
                var startTimeParam = new SqlParameter("@StartTime", startTime);
                var endTimeOutParam = new SqlParameter
                {
                    ParameterName = "@EndTime",
                    SqlDbType = SqlDbType.Time,
                    Direction = ParameterDirection.Output
                };
                var errorMsgParam = new SqlParameter
                {
                    ParameterName = "@ErrorMessage",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 200,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspApptCheckSchedule @DoctorId, @DayOfWeek, @StartTime, @EndTime OUTPUT, @ErrorMessage OUTPUT",
                    doctorIdParam, dayOfWeekParam, startTimeParam, endTimeOutParam, errorMsgParam);

                var scheduleError = errorMsgParam.Value as string;
                if (!string.IsNullOrEmpty(scheduleError))
                    return Result<string>.Fail(scheduleError);

                var endTime = (TimeSpan)endTimeOutParam.Value;

                // BR2 CHECK: no double booking
                var conflictDoctorIdParam = new SqlParameter("@DoctorId", dto.DoctorId);
                var conflictDateParam = new SqlParameter("@AppointmentDate", dto.AppointmentDate.Date);
                var conflictStartParam = new SqlParameter("@StartTime", startTime);
                var conflictEndParam = new SqlParameter("@EndTime", endTime);
                var hasConflictParam = new SqlParameter
                {
                    ParameterName = "@HasConflict",
                    SqlDbType = SqlDbType.Bit,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspApptCheckConflict @DoctorId, @AppointmentDate, @StartTime, @EndTime, @HasConflict OUTPUT",
                    conflictDoctorIdParam, conflictDateParam, conflictStartParam, conflictEndParam, hasConflictParam);

                var hasConflict = (bool)(hasConflictParam.Value ?? false);
                if (hasConflict)
                    return Result<string>.Fail(
                        "This time slot is already booked for the doctor. Please choose a different slot.");

                // BR1: next appointment number
                var nextNumberParam = new SqlParameter
                {
                    ParameterName = "@NextNumber",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 20,
                    Direction = ParameterDirection.Output
                };
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspApptNextNumber @NextNumber OUTPUT", nextNumberParam);
                var appointmentNumber = nextNumberParam.Value?.ToString() ?? "APT-000001";

                // save
                var saveNumberParam = new SqlParameter("@AppointmentNumber", appointmentNumber);
                var savePatientIdParam = new SqlParameter("@PatientId", dto.PatientId);
                var saveDoctorIdParam = new SqlParameter("@DoctorId", dto.DoctorId);
                var saveDepartmentIdParam = new SqlParameter("@DepartmentId", dto.DepartmentId);
                var saveDateParam = new SqlParameter("@AppointmentDate", dto.AppointmentDate.Date);
                var saveStartParam = new SqlParameter("@StartTime", startTime);
                var saveEndParam = new SqlParameter("@EndTime", endTime);
                var saveComplaintParam = new SqlParameter("@ChiefComplaint", dto.ChiefComplaint.Trim());
                var saveCreatedByParam = new SqlParameter("@LastUpdatedBy", createdByUserId);
                var newIdParam = new SqlParameter
                {
                    ParameterName = "@NewId",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 36,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspApptSave @AppointmentNumber, @PatientId, @DoctorId, @DepartmentId, @AppointmentDate, @StartTime, @EndTime, @ChiefComplaint, @LastUpdatedBy, @NewId OUTPUT",
                    saveNumberParam, savePatientIdParam, saveDoctorIdParam, saveDepartmentIdParam, saveDateParam,
                    saveStartParam, saveEndParam, saveComplaintParam, saveCreatedByParam, newIdParam);

                var newId = newIdParam.Value?.ToString() ?? string.Empty;

                _logger.LogInformation("Appointment created: {Number} for Patient {PatientId}", appointmentNumber, dto.PatientId);

                return Result<string>.Ok(newId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment {@Dto}", dto);
                return Result<string>.Fail("Failed to create appointment");
            }
        }

        // ── GET AVAILABLE SLOTS ───────────────────────────────
        public async Task<Result<List<AppointmentSlotDto>>>
            GetAvailableSlotsAsync(string doctorId, DateTime date)
        {
            try
            {
                var dayOfWeek = date.DayOfWeek;

                var doctorIdParam = new SqlParameter("@DoctorId", doctorId);
                var dayOfWeekParam = new SqlParameter("@DayOfWeek", (int)dayOfWeek);

                var scheduleResults = await _context.Database
                    .SqlQueryRaw<AppointmentScheduleResultDto>(
                        "EXEC udspDoctorSchedulesByDoctorAndDay @DoctorId, @DayOfWeek",
                        doctorIdParam, dayOfWeekParam)
                    .ToListAsync();

                var schedule = scheduleResults.FirstOrDefault();
                if (schedule == null)
                    return Result<List<AppointmentSlotDto>>.Fail("Doctor is not available on this day");

                var allSlots = GenerateSlots(schedule.StartTime, schedule.EndTime, schedule.SlotDurationMinutes);

                var bookedDoctorIdParam = new SqlParameter("@DoctorId", doctorId);
                var bookedDateParam = new SqlParameter("@AppointmentDate", date.Date);

                var bookedSlots = await _context.Database
                    .SqlQueryRaw<TimeSpan>(
                        "EXEC udspApptBookedStarts @DoctorId, @AppointmentDate",
                        bookedDoctorIdParam, bookedDateParam)
                    .ToListAsync();

                var now = DateTime.Now;
                var availableSlots = allSlots
                    .Where(slot =>
                        !bookedSlots.Contains(slot.Start) &&
                        (date.Date != DateTime.Today || date.Date + slot.Start > now))
                    .Select(slot => new AppointmentSlotDto
                    {
                        StartTime = slot.Start.ToString(@"hh\:mm"),
                        EndTime = slot.End.ToString(@"hh\:mm")
                    })
                    .ToList();

                if (!availableSlots.Any())
                    return Result<List<AppointmentSlotDto>>.Fail("No available slots for this date");

                return Result<List<AppointmentSlotDto>>.Ok(availableSlots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slots for doctor {DoctorId}", doctorId);
                return Result<List<AppointmentSlotDto>>.Fail("Failed to get available slots");
            }
        }

        // ── CANCEL ────────────────────────────────────────────
        public async Task<Result> CancelAppointmentAsync(string id, string cancelReason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cancelReason))
                    return Result.Fail("Cancellation reason is required");

                var idParam = new SqlParameter("@Id", id);
                var reasonParam = new SqlParameter("@CancelReason", cancelReason.Trim());
                var updatedParam = new SqlParameter
                {
                    ParameterName = "@Updated",
                    SqlDbType = SqlDbType.Bit,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspApptCancel @Id, @CancelReason, @Updated OUTPUT",
                    idParam, reasonParam, updatedParam);

                var updated = (bool)(updatedParam.Value ?? false);
                if (!updated)
                {
                    var current = await GetAppointmentByIdAsync(id);
                    if (!current.IsSuccess)
                        return Result.Fail("Appointment not found");
                    return Result.Fail($"Cannot cancel appointment with status: {current.Data!.Status}");
                }

                _logger.LogInformation("Appointment cancelled: {Id}", id);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment {Id}", id);
                return Result.Fail("Failed to cancel appointment");
            }
        }

        // ── CHECK-IN ────────────────────────────────────────────
        public async Task<Result> CheckInAppointmentAsync(string id)
            => await RunTransitionAsync(id, "udspApptCheckIn", "checked in");

        // ── NO-SHOW ─────────────────────────────────────────────
        public async Task<Result> MarkNoShowAsync(string id)
            => await RunTransitionAsync(id, "udspApptNoShow", "marked no-show");

        // ── COMPLETE ────────────────────────────────────────────
        // Ownership: only assigned doctor (or Admin) can complete
        public async Task<Result> CompleteAppointmentAsync(string id, string currentUserId, bool isAdmin)
        {
            try
            {
                if (!isAdmin)
                {
                    var appointment = await GetAppointmentByIdAsync(id);
                    if (!appointment.IsSuccess)
                        return Result.Fail("Appointment not found");

                    var userIdParam = new SqlParameter("@ApplicationUserId", currentUserId);
                    var doctorIdOutParam = new SqlParameter
                    {
                        ParameterName = "@DoctorId",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 36,
                        Direction = ParameterDirection.Output
                    };

                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC udspApptGetDoctorIdByUser @ApplicationUserId, @DoctorId OUTPUT",
                        userIdParam, doctorIdOutParam);

                    var callerDoctorId = doctorIdOutParam.Value?.ToString();
                    if (string.IsNullOrEmpty(callerDoctorId) || callerDoctorId != appointment.Data!.DoctorId)
                        return Result.Fail("You can only complete your own appointments");
                }

                return await RunTransitionAsync(id, "udspApptComplete", "completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing appointment {Id}", id);
                return Result.Fail("Failed to complete appointment");
            }
        }

        // ── DOCTOR DASHBOARD (BR10) ─────────────────────────────
        public async Task<Result<List<AppointmentListItemDto>>>
            GetDoctorAppointmentsAsync(string doctorUserId, DateTime? date)
        {
            try
            {
                var userIdParam = new SqlParameter("@ApplicationUserId", doctorUserId);
                var doctorIdOutParam = new SqlParameter
                {
                    ParameterName = "@DoctorId",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 36,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspApptGetDoctorIdByUser @ApplicationUserId, @DoctorId OUTPUT",
                    userIdParam, doctorIdOutParam);

                var doctorId = doctorIdOutParam.Value?.ToString();
                if (string.IsNullOrEmpty(doctorId))
                    return Result<List<AppointmentListItemDto>>.Fail("No doctor profile found for this user");

                var listDoctorIdParam = new SqlParameter("@DoctorId", doctorId);
                var dateParam = new SqlParameter("@Date", (object?)date?.Date ?? DBNull.Value);

                var items = await _context.Set<AppointmentListItemDto>()
                    .FromSqlRaw("EXEC udspApptByDoctor @DoctorId, @Date", listDoctorIdParam, dateParam)
                    .ToListAsync();

                return Result<List<AppointmentListItemDto>>.Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctor appointments for user {UserId}", doctorUserId);
                return Result<List<AppointmentListItemDto>>.Fail("Failed to fetch appointments");
            }
        }

        // ── PRIVATE: shared status-transition runner ──────────
        private async Task<Result> RunTransitionAsync(string id, string spName, string actionLabel)
        {
            try
            {
                var idParam = new SqlParameter("@Id", id);
                var updatedParam = new SqlParameter
                {
                    ParameterName = "@Updated",
                    SqlDbType = SqlDbType.Bit,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    $"EXEC {spName} @Id, @Updated OUTPUT", idParam, updatedParam);

                var updated = (bool)(updatedParam.Value ?? false);
                if (!updated)
                {
                    var current = await GetAppointmentByIdAsync(id);
                    if (!current.IsSuccess)
                        return Result.Fail("Appointment not found");
                    return Result.Fail($"Cannot change status from {current.Data!.Status} to {actionLabel}");
                }

                _logger.LogInformation("Appointment {Action}: {Id}", actionLabel, id);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running {SpName} on appointment {Id}", spName, id);
                return Result.Fail($"Failed to mark appointment as {actionLabel}");
            }
        }

        // ── SLOT GENERATION HELPER ────
        private List<(TimeSpan Start, TimeSpan End)> GenerateSlots(
            TimeSpan scheduleStart, TimeSpan scheduleEnd, int slotDurationMinutes)
        {
            var slots = new List<(TimeSpan Start, TimeSpan End)>();
            var current = scheduleStart;
            var slotDuration = TimeSpan.FromMinutes(slotDurationMinutes);

            while (current + slotDuration <= scheduleEnd)
            {
                slots.Add((current, current + slotDuration));
                current += slotDuration;
            }
            return slots;
        }
    }
}