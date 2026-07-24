using ClinicMS.Application.DTOs.DoctorSchedule;
using ClinicMS.Application.Interfaces;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicMS.Infrastructure.Services
{
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DoctorScheduleService> _logger;

        public DoctorScheduleService(ApplicationDbContext context, ILogger<DoctorScheduleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── GET BY DOCTOR ──────────────────────────────────────
        public async Task<Result<List<DoctorScheduleListItemDto>>>
            GetByDoctorAsync(string doctorId)
        {
            try
            {
                var doctorIdParam = new SqlParameter("@DoctorId", doctorId);

                var list = await _context.DoctorScheduleListItems
                    .FromSqlRaw("EXEC udspDoctorSchedulesByDoctor @DoctorId", doctorIdParam)
                    .ToListAsync();

                return Result<List<DoctorScheduleListItemDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schedules for doctor {DoctorId}", doctorId);
                return Result<List<DoctorScheduleListItemDto>>.Fail("Failed to fetch schedules");
            }
        }

        // ── GET BY ID ───────────────────────────────────────────
        public async Task<Result<DoctorScheduleResponseDto>>
            GetByIdAsync(string id)
        {
            try
            {
                var idParam = new SqlParameter("@Id", id);

                var results = await _context.DoctorScheduleResponses
                    .FromSqlRaw("EXEC udspDoctorSchedulesGetById @Id", idParam)
                    .ToListAsync();

                var dto = results.FirstOrDefault();
                if (dto == null)
                    return Result<DoctorScheduleResponseDto>.Fail("Schedule not found");

                return Result<DoctorScheduleResponseDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schedule {Id}", id);
                return Result<DoctorScheduleResponseDto>.Fail("Failed to fetch schedule");
            }
        }

        // ── SAVE (create or update) ──────────────────────────────
        public async Task<Result<string>>
            SaveAsync(DoctorScheduleRequestDto dto)
        {
            try
            {
                if (dto.EndTime <= dto.StartTime)
                    return Result<string>.Fail("End time must be after start time");

                var overlap = await CheckOverlapAsync(dto.DoctorId, dto.DayOfWeek, dto.StartTime, dto.EndTime, dto.Id);
                if (overlap)
                    return Result<string>.Fail("Schedule overlaps an existing block for this day");

                if (string.IsNullOrEmpty(dto.Id))
                {
                    var doctorIdParam = new SqlParameter("@DoctorId", dto.DoctorId);
                    var dayOfWeekParam = new SqlParameter("@DayOfWeek", (int)dto.DayOfWeek);
                    var startTimeParam = new SqlParameter("@StartTime", dto.StartTime);
                    var endTimeParam = new SqlParameter("@EndTime", dto.EndTime);
                    var slotDurationParam = new SqlParameter("@SlotDurationMinutes", dto.SlotDurationMinutes);

                    var newIdParam = new SqlParameter
                    {
                        ParameterName = "@NewId",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 36,
                        Direction = ParameterDirection.Output
                    };

                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC udspDoctorSchedulesSave @DoctorId, @DayOfWeek, @StartTime, @EndTime, @SlotDurationMinutes, @NewId OUTPUT",
                        doctorIdParam, dayOfWeekParam, startTimeParam, endTimeParam, slotDurationParam, newIdParam);

                    var newId = newIdParam.Value?.ToString() ?? string.Empty;

                    _logger.LogInformation("Schedule created: {Id} for doctor {DoctorId}", newId, dto.DoctorId);

                    return Result<string>.Ok(newId);
                }
                else
                {
                    var idParam = new SqlParameter("@Id", dto.Id);
                    var dayOfWeekParam = new SqlParameter("@DayOfWeek", (int)dto.DayOfWeek);
                    var startTimeParam = new SqlParameter("@StartTime", dto.StartTime);
                    var endTimeParam = new SqlParameter("@EndTime", dto.EndTime);
                    var slotDurationParam = new SqlParameter("@SlotDurationMinutes", dto.SlotDurationMinutes);

                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC udspDoctorSchedulesUpdate @Id, @DayOfWeek, @StartTime, @EndTime, @SlotDurationMinutes",
                        idParam, dayOfWeekParam, startTimeParam, endTimeParam, slotDurationParam);

                    _logger.LogInformation("Schedule updated: {Id}", dto.Id);

                    return Result<string>.Ok(dto.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving schedule: {@Dto}", dto);
                return Result<string>.Fail("Failed to save schedule");
            }
        }

        // ── DELETE ────────────────────────────────────────────
        public async Task<Result>
            DeleteAsync(string id)
        {
            try
            {
                var idParam = new SqlParameter("@Id", id);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspDoctorSchedulesDelete @Id", idParam);

                _logger.LogInformation("Schedule deleted: {Id}", id);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule {Id}", id);
                return Result.Fail("Failed to delete schedule");
            }
        }

        // ── PRIVATE HELPERS ───────────────────────────────────
        private async Task<bool>
            CheckOverlapAsync(string doctorId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, string? excludeId)
        {
            var doctorIdParam = new SqlParameter("@DoctorId", doctorId);
            var dayOfWeekParam = new SqlParameter("@DayOfWeek", (int)dayOfWeek);
            var startTimeParam = new SqlParameter("@StartTime", startTime);
            var endTimeParam = new SqlParameter("@EndTime", endTime);
            var excludeIdParam = new SqlParameter("@ExcludeId", (object?)excludeId ?? DBNull.Value);

            var overlapParam = new SqlParameter
            {
                ParameterName = "@HasOverlap",
                SqlDbType = SqlDbType.Bit,
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC udspDoctorSchedulesCheckOverlap @DoctorId, @DayOfWeek, @StartTime, @EndTime, @ExcludeId, @HasOverlap OUTPUT",
                doctorIdParam, dayOfWeekParam, startTimeParam, endTimeParam, excludeIdParam, overlapParam);

            return (bool)(overlapParam.Value ?? false);
        }
    }
}