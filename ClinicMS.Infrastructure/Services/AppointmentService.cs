using ClinicMS.Application.DTOs.Appointment;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Domain.Enums;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicMS.Infrastructure.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            ApplicationDbContext context,
            ILogger<AppointmentService> logger)
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
                var query = _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.Department)
                    .AsQueryable();

                // Apply filters — each block adds WHERE clause
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    query = query.Where(a =>
                        a.AppointmentNumber.Contains(filter.SearchTerm) ||
                        a.Patient!.FirstName.Contains(filter.SearchTerm) ||
                        a.Patient!.LastName.Contains(filter.SearchTerm));

                if (!string.IsNullOrWhiteSpace(filter.DoctorId))
                    query = query.Where(a =>
                        a.DoctorId == filter.DoctorId);

                if (!string.IsNullOrWhiteSpace(filter.DepartmentId))
                    query = query.Where(a =>
                        a.DepartmentId == filter.DepartmentId);

                if (filter.Status.HasValue)
                    query = query.Where(a =>
                        a.Status == filter.Status.Value);

                if (filter.FromDate.HasValue)
                    query = query.Where(a =>
                        a.AppointmentDate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(a =>
                        a.AppointmentDate <= filter.ToDate.Value);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenBy(a => a.StartTime)
                    // Most recent appointments first
                    // Within same date → ordered by time
                    .Skip((filter.PageNo - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(a => new AppointmentListItemDto
                    {
                        Id = a.Id,
                        AppointmentNumber = a.AppointmentNumber,
                        PatientName = a.Patient!.FirstName
                            + " " + a.Patient.LastName,
                        PatientNumber = a.Patient.PatientNumber,
                        DoctorName = a.Doctor!.FullName,
                        DepartmentName = a.Department!.Name,
                        AppointmentDate = a.AppointmentDate,
                        // Format TimeSpan as "09:00" string for display
                        StartTime = a.StartTime.ToString(@"hh\:mm"),
                        EndTime = a.EndTime.ToString(@"hh\:mm"),
                        // @"hh\:mm" = verbatim string, \ escapes the colon
                        // Result: "09:00", "13:30" etc
                        Status = a.Status,
                        ChiefComplaint = a.ChiefComplaint
                    })
                    .ToListAsync();

                return Result<PaginatedResult<AppointmentListItemDto>>
                    .Ok(new PaginatedResult<AppointmentListItemDto>
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
                return Result<PaginatedResult<AppointmentListItemDto>>
                    .Fail("Failed to fetch appointments");
            }
        }

        // ── GET BY ID ─────────────────────────────────────────
        public async Task<Result<AppointmentResponseDto>>
            GetAppointmentByIdAsync(string id)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.Department)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                    return Result<AppointmentResponseDto>
                        .Fail("Appointment not found");

                return Result<AppointmentResponseDto>.Ok(
                    MapToResponseDto(appointment));
                // MapToResponseDto = private helper below
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching appointment {Id}", id);
                return Result<AppointmentResponseDto>
                    .Fail("Failed to fetch appointment");
            }
        }

        // ── CREATE APPOINTMENT ────────────────────────────────
        // enforces BR1 + BR2 + BR3
        public async Task<Result<string>> CreateAppointmentAsync(
            AppointmentRequestDto dto,
            string createdByUserId)
        {
            try
            {
                // Parse StartTime string "09:00" → TimeSpan
                // TimeSpan.Parse("09:00") = TimeSpan{09:00:00}
                if (!TimeSpan.TryParse(dto.StartTime,
                    out TimeSpan startTime))
                    return Result<string>
                        .Fail("Invalid start time format");

                // ── BR3 CHECK 1: Not in the past ──────────────
                var appointmentDateTime = dto.AppointmentDate
                    .Date + startTime;

                if (appointmentDateTime < DateTime.Now)
                    return Result<string>
                        .Fail("Cannot book appointment in the past");

                // ── BR3 CHECK 2: Within doctor's schedule ─────
                var scheduleCheck = await EnsureWithinScheduleAsync(
                    dto.DoctorId,
                    dto.AppointmentDate,
                    startTime);

                if (!scheduleCheck.IsSuccess)
                    return Result<string>
                        .Fail(scheduleCheck.ErrorMessage);

                // EndTime = StartTime + SlotDurationMinutes
                // from the schedule row found above
                var endTime = scheduleCheck.Data;

                // ── BR2 CHECK: No double booking ──────────────
                var conflictCheck = await EnsureNoConflictAsync(
                    dto.DoctorId,
                    dto.AppointmentDate,
                    startTime,
                    endTime);

                if (!conflictCheck.IsSuccess)
                    return Result<string>
                        .Fail(conflictCheck.ErrorMessage);

                // ── BR1: Generate AppointmentNumber ───────────
                var appointmentNumber =
                    await GenerateAppointmentNumberAsync();

                // All checks passed → create appointment
                var appointment = new Appointment
                {
                    Id = Guid.NewGuid().ToString(),
                    AppointmentNumber = appointmentNumber,
                    PatientId = dto.PatientId,
                    DoctorId = dto.DoctorId,
                    DepartmentId = dto.DepartmentId,
                    AppointmentDate = dto.AppointmentDate.Date,
                    StartTime = startTime,
                    EndTime = endTime,
                    Status = AppointmentStatus.Scheduled,
                    ChiefComplaint = dto.ChiefComplaint.Trim(),
                    CreatedOn = DateTime.UtcNow,
                    CreatedById = createdByUserId
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Appointment created: {Number} for Patient {PatientId}",
                    appointmentNumber, dto.PatientId);

                return Result<string>.Ok(appointment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating appointment {@Dto}", dto);
                return Result<string>
                    .Fail("Failed to create appointment");
            }
        }

        // ── GET AVAILABLE SLOTS ───────────────────────────────
        // BR3 logic — compute available slots for booking form
        // Called by cascading dropdown: Doctor + Date selected
        public async Task<Result<List<AppointmentSlotDto>>>
            GetAvailableSlotsAsync(string doctorId, DateTime date)
        {
            try
            {
                // Step 1: Find doctor's schedule for this day of week
                // DayOfWeek = Sunday=0, Monday=1...Saturday=6
                var dayOfWeek = date.DayOfWeek;

                var schedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(s =>
                        s.DoctorId == doctorId &&
                        s.DayOfWeek == dayOfWeek &&
                        s.IsActive);

                if (schedule == null)
                    return Result<List<AppointmentSlotDto>>
                        .Fail("Doctor is not available on this day");

                // Step 2: Generate ALL possible slots for this day
                // Example: StartTime=09:00, EndTime=13:00, Slot=30min
                // Slots: 09:00-09:30, 09:30-10:00, 10:00-10:30...12:30-13:00
                var allSlots = GenerateSlots(
                    schedule.StartTime,
                    schedule.EndTime,
                    schedule.SlotDurationMinutes);

                // Step 3: Get already-booked slots for this doctor+date
                // Ignore Cancelled + NoShow — those slots are free again
                var bookedSlots = await _context.Appointments
                    .Where(a =>
                        a.DoctorId == doctorId &&
                        a.AppointmentDate.Date == date.Date &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow)
                    .Select(a => a.StartTime)
                    .ToListAsync();
                // Select only StartTime → List<TimeSpan>
                // bookedSlots = ["09:00", "10:30"] etc

                // Step 4: Remove booked slots + past slots
                var now = DateTime.Now;
                var availableSlots = allSlots
                    .Where(slot =>
                        // Not already booked
                        !bookedSlots.Contains(slot.Start) &&
                        // Not in the past (for today's date)
                        (date.Date != DateTime.Today ||
                         date.Date + slot.Start > now))
                    .Select(slot => new AppointmentSlotDto
                    {
                        StartTime = slot.Start.ToString(@"hh\:mm"),
                        EndTime = slot.End.ToString(@"hh\:mm")
                    })
                    .ToList();

                if (!availableSlots.Any())
                    return Result<List<AppointmentSlotDto>>
                        .Fail("No available slots for this date");

                return Result<List<AppointmentSlotDto>>
                    .Ok(availableSlots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting slots for doctor {DoctorId}", doctorId);
                return Result<List<AppointmentSlotDto>>
                    .Fail("Failed to get available slots");
            }
        }

        // ── CANCEL ────────────────────────────────────────────
        public async Task<Result> CancelAppointmentAsync(
            string id, string cancelReason)
        {
            try
            {
                // BR8: reason required
                if (string.IsNullOrWhiteSpace(cancelReason))
                    return Result.Fail(
                        "Cancellation reason is required");

                var appointment = await _context.Appointments
                    .FindAsync(id);

                if (appointment == null)
                    return Result.Fail("Appointment not found");

                // BR4: only Scheduled or CheckedIn can be cancelled
                if (appointment.Status != AppointmentStatus.Scheduled
                    && appointment.Status != AppointmentStatus.CheckedIn)
                    return Result.Fail(
                        $"Cannot cancel appointment with status: " +
                        $"{appointment.Status}");

                appointment.Status = AppointmentStatus.Cancelled;
                appointment.CancelReason = cancelReason.Trim();

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Appointment cancelled: {Id}", id);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error cancelling appointment {Id}", id);
                return Result.Fail("Failed to cancel appointment");
            }
        }

        // ══════════════════════════════════════════════════════
        // PRIVATE HELPERS — business rule implementations
        // ══════════════════════════════════════════════════════

        // ── BR3 HELPER: Check slot within schedule ────────────
        private async Task<Result<TimeSpan>>
            EnsureWithinScheduleAsync(
                string doctorId,
                DateTime date,
                TimeSpan startTime)
        {
            var schedule = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s =>
                    s.DoctorId == doctorId &&
                    s.DayOfWeek == date.DayOfWeek &&
                    s.IsActive);

            if (schedule == null)
                return Result<TimeSpan>.Fail(
                    "Doctor has no schedule for this day");

            // Check startTime falls within schedule window
            if (startTime < schedule.StartTime ||
                startTime >= schedule.EndTime)
                return Result<TimeSpan>.Fail(
                    $"Slot must be between " +
                    $"{schedule.StartTime:hh\\:mm} and " +
                    $"{schedule.EndTime:hh\\:mm}");

            // Check slot aligns to SlotDurationMinutes
            // e.g. 30min slots: 09:00✓ 09:15✗ 09:30✓
            var minutesFromStart =
                (startTime - schedule.StartTime).TotalMinutes;
            if (minutesFromStart % schedule.SlotDurationMinutes != 0)
                return Result<TimeSpan>.Fail(
                    "Slot must align to schedule intervals");

            // Compute EndTime = StartTime + SlotDuration
            var endTime = startTime.Add(
                TimeSpan.FromMinutes(schedule.SlotDurationMinutes));

            // Ensure EndTime doesn't exceed schedule EndTime
            if (endTime > schedule.EndTime)
                return Result<TimeSpan>.Fail(
                    "Slot extends beyond doctor's schedule");

            // Return endTime so CreateAppointment can use it
            return Result<TimeSpan>.Ok(endTime);
        }

        // ── BR2 HELPER: No double booking ────────────────────
        private async Task<Result> EnsureNoConflictAsync(
            string doctorId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            // Find any existing appointment that overlaps
            // Two time ranges overlap when:
            // startA < endB AND startB < endA
            // This is the standard overlap formula
            //
            // Example:
            // Existing: 09:00-09:30
            // New:      09:15-09:45
            // 09:15 < 09:30 ✓ AND 09:00 < 09:45 ✓ → OVERLAP → reject


            var hasConflict = await _context.Appointments
                .AnyAsync(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate.Date == date.Date &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.NoShow &&
                    // Overlap formula:
                    a.StartTime < endTime &&
                    startTime < a.EndTime);

            if (hasConflict)
                return Result.Fail(
                    "This time slot is already booked for the doctor. " +
                    "Please choose a different slot.");

            return Result.Ok();
        }

        // ── BR1 HELPER: Generate AppointmentNumber ────────────
        private async Task<string> GenerateAppointmentNumberAsync()
        {
            // Find highest existing number
            // APT-000045 → extract 45 → next = 46 → APT-000046
            var lastAppointment = await _context.Appointments
                .OrderByDescending(a => a.AppointmentNumber)
                .Select(a => a.AppointmentNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1; // start from 1 if no appointments yet

            if (!string.IsNullOrEmpty(lastAppointment))
            {
                // Split "APT-000045" by "-" → ["APT", "000045"]
                // Take last part "000045" → parse to int 45
                var parts = lastAppointment.Split('-');
                if (parts.Length == 2 &&
                    int.TryParse(parts[1], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"APT-{nextNumber.ToString().PadLeft(6, '0')}";
        }

        // ── SLOT GENERATION HELPER ────────────────────────────
        // Generates all time slots between start and end
        // stepped by slotDuration minutes
        private List<(TimeSpan Start, TimeSpan End)> GenerateSlots(
            TimeSpan scheduleStart,
            TimeSpan scheduleEnd,
            int slotDurationMinutes)
        {
            // (TimeSpan Start, TimeSpan End) = C# tuple
            var slots = new List<(TimeSpan Start, TimeSpan End)>();

            var current = scheduleStart;
            var slotDuration = TimeSpan
                .FromMinutes(slotDurationMinutes);

            // Walk from start to end, step by slotDuration
            while (current + slotDuration <= scheduleEnd)
            {
                slots.Add((current, current + slotDuration));
                current = current + slotDuration;
            }

            return slots;
        }

        // ── MAPPING HELPER ────────────────────────────────────
        private AppointmentResponseDto MapToResponseDto(
            Appointment a)
        {
            return new AppointmentResponseDto
            {
                Id = a.Id,
                AppointmentNumber = a.AppointmentNumber,
                PatientId = a.PatientId,
                PatientName = a.Patient != null
                    ? $"{a.Patient.FirstName} {a.Patient.LastName}"
                    : string.Empty,
                PatientNumber = a.Patient?.PatientNumber
                    ?? string.Empty,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor?.FullName ?? string.Empty,
                DepartmentId = a.DepartmentId,
                DepartmentName = a.Department?.Name ?? string.Empty,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime.ToString(@"hh\:mm"),
                EndTime = a.EndTime.ToString(@"hh\:mm"),
                Status = a.Status,
                ChiefComplaint = a.ChiefComplaint,
                CancelReason = a.CancelReason,
                CreatedOn = a.CreatedOn
            };
        }

        // ── CHECK-IN ────────────────────────────────────────────
        // BR4: Scheduled → CheckedIn only. Admin/Receptionist role — no ownership check needed.
        public async Task<Result> CheckInAppointmentAsync(string id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);

                if (appointment == null)
                    return Result.Fail("Appointment not found");

                // BR4 guard — reuse shared transition-check helper (see below)
                var transitionCheck = EnsureValidTransition(
                    appointment.Status, AppointmentStatus.CheckedIn);

                if (!transitionCheck.IsSuccess)
                    return transitionCheck;

                appointment.Status = AppointmentStatus.CheckedIn;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Appointment checked in: {Id}", id);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in appointment {Id}", id);
                return Result.Fail("Failed to check in appointment");
            }
        }

        // ── COMPLETE ────────────────────────────────────────────
        // BR4: Scheduled|CheckedIn → Completed.
        // Ownership: only assigned doctor (or Admin) can complete.
        public async Task<Result> CompleteAppointmentAsync(
            string id, string currentUserId, bool isAdmin)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);

                if (appointment == null)
                    return Result.Fail("Appointment not found");

                // ── OWNERSHIP CHECK (only for non-Admin) ──────────
                // Admin bypasses ownership — can complete any appointment.
                // Non-Admin (Doctor) must match: their logged-in ApplicationUserId
                // must resolve to the SAME Doctor.Id as appointment.DoctorId.
                if (!isAdmin)
                {
                    var doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.ApplicationUserId == currentUserId);

                    // doctor == null → the logged-in user has no matching Doctor record at all
                    if (doctor == null || doctor.Id != appointment.DoctorId)
                        return Result.Fail(
                            "You can only complete your own appointments");
                }

                // BR4 guard
                var transitionCheck = EnsureValidTransition(
                    appointment.Status, AppointmentStatus.Completed);

                if (!transitionCheck.IsSuccess)
                    return transitionCheck;

                appointment.Status = AppointmentStatus.Completed;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Appointment completed: {Id}", id);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing appointment {Id}", id);
                return Result.Fail("Failed to complete appointment");
            }
        }

        // ── NO-SHOW ─────────────────────────────────────────────
        // BR4: Scheduled → NoShow only. Admin/Receptionist — no ownership check.
        public async Task<Result> MarkNoShowAsync(string id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);

                if (appointment == null)
                    return Result.Fail("Appointment not found");

                var transitionCheck = EnsureValidTransition(
                    appointment.Status, AppointmentStatus.NoShow);

                if (!transitionCheck.IsSuccess)
                    return transitionCheck;

                appointment.Status = AppointmentStatus.NoShow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Appointment marked no-show: {Id}", id);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking no-show {Id}", id);
                return Result.Fail("Failed to mark appointment as no-show");
            }
        }

        // ── DOCTOR DASHBOARD (BR10) ─────────────────────────────
        // Returns only appointments belonging to the doctor identified by doctorUserId
        public async Task<Result<List<AppointmentListItemDto>>>
     GetDoctorAppointmentsAsync(string doctorUserId, DateTime? date)
        {
            try
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.ApplicationUserId == doctorUserId);

                if (doctor == null)
                    return Result<List<AppointmentListItemDto>>
                        .Fail("No doctor profile found for this user");

                var query = _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.Department)
                    .Where(a => a.DoctorId == doctor.Id);

                if (date.HasValue)
                {
                    // Specific date requested → just that day
                    query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
                }

                var items = await query
                    .OrderByDescending(a => a.AppointmentDate)   // most recent first
                    .ThenBy(a => a.StartTime)
                    .Select(a => new AppointmentListItemDto
                    {
                        Id = a.Id,
                        AppointmentNumber = a.AppointmentNumber,
                        PatientName = a.Patient!.FirstName + " " + a.Patient.LastName,
                        PatientNumber = a.Patient.PatientNumber,
                        DoctorName = a.Doctor!.FullName,
                        DepartmentName = a.Department!.Name,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime.ToString(@"hh\:mm"),
                        EndTime = a.EndTime.ToString(@"hh\:mm"),
                        Status = a.Status,
                        ChiefComplaint = a.ChiefComplaint
                    })
                    .ToListAsync();

                return Result<List<AppointmentListItemDto>>.Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctor appointments for user {UserId}", doctorUserId);
                return Result<List<AppointmentListItemDto>>.Fail("Failed to fetch appointments");
            }
        }

        // ══════════════════════════════════════════════════════
        // PRIVATE HELPER — BR4 shared transition guard
        // ══════════════════════════════════════════════════════
        private Result EnsureValidTransition(
            AppointmentStatus currentStatus, AppointmentStatus targetStatus)
        {

            bool isValid = (currentStatus, targetStatus) switch
            {
                (AppointmentStatus.Scheduled, AppointmentStatus.CheckedIn) => true,
                (AppointmentStatus.Scheduled, AppointmentStatus.Completed) => true,
                (AppointmentStatus.CheckedIn, AppointmentStatus.Completed) => true,
                (AppointmentStatus.Scheduled, AppointmentStatus.NoShow) => true,
                _ => false
            };

            if (!isValid)
                return Result.Fail(
                    $"Cannot change status from {currentStatus} to {targetStatus}");

            return Result.Ok();
        }


    }
}