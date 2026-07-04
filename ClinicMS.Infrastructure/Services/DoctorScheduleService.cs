using ClinicMS.Application.DTOs.DoctorSchedule;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public async Task<Result<List<DoctorScheduleListItemDto>>> GetByDoctorAsync(string doctorId)
        {
            try
            {
                var list = await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctorId)
                    .OrderBy(s => s.DayOfWeek)
                    .Select(s => new DoctorScheduleListItemDto
                    {
                        Id = s.Id,
                        DayOfWeek = s.DayOfWeek.ToString(),
                        StartTime = s.StartTime.ToString(@"hh\:mm"),
                        EndTime = s.EndTime.ToString(@"hh\:mm"),
                        SlotDurationMinutes = s.SlotDurationMinutes,
                        IsActive = s.IsActive
                    }).ToListAsync();

                return Result<List<DoctorScheduleListItemDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schedules for doctor {DoctorId}", doctorId);
                return Result<List<DoctorScheduleListItemDto>>.Fail("Failed to fetch schedules");
            }
        }

        public async Task<Result<DoctorScheduleResponseDto>> GetByIdAsync(string id)
        {
            try
            {
                var s = await _context.DoctorSchedules.FindAsync(id);
                if (s == null)
                    return Result<DoctorScheduleResponseDto>.Fail("Schedule not found");

                var dto = new DoctorScheduleResponseDto
                {
                    Id = s.Id,
                    DoctorId = s.DoctorId,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    SlotDurationMinutes = s.SlotDurationMinutes,
                    IsActive = s.IsActive
                };

                return Result<DoctorScheduleResponseDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schedule {Id}", id);
                return Result<DoctorScheduleResponseDto>.Fail("Failed to fetch schedule");
            }
        }

        public async Task<Result<string>> SaveAsync(DoctorScheduleRequestDto dto)
        {
            if (dto.EndTime <= dto.StartTime)
                return Result<string>.Fail("End time must be after start time");

            var overlap = await _context.DoctorSchedules.AnyAsync(s =>
                s.DoctorId == dto.DoctorId &&
                s.DayOfWeek == dto.DayOfWeek &&
                s.Id != dto.Id &&
                dto.StartTime < s.EndTime && dto.EndTime > s.StartTime);

            if (overlap)
                return Result<string>.Fail("Schedule overlaps an existing block for this day");

            if (string.IsNullOrEmpty(dto.Id))
            {
                var entity = new DoctorSchedule
                {
                    Id = Guid.NewGuid().ToString(),
                    DoctorId = dto.DoctorId,
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    SlotDurationMinutes = dto.SlotDurationMinutes,
                    IsActive = true
                };
                _context.DoctorSchedules.Add(entity);
                await _context.SaveChangesAsync();
                return Result<string>.Ok(entity.Id);
            }
            else
            {
                var entity = await _context.DoctorSchedules.FindAsync(dto.Id);
                if (entity == null) return Result<string>.Fail("Schedule not found");

                entity.DayOfWeek = dto.DayOfWeek;
                entity.StartTime = dto.StartTime;
                entity.EndTime = dto.EndTime;
                entity.SlotDurationMinutes = dto.SlotDurationMinutes;
                await _context.SaveChangesAsync();
                return Result<string>.Ok(entity.Id);
            }
        }

        public async Task<Result> DeleteAsync(string id)
        {
            var entity = await _context.DoctorSchedules.FindAsync(id);
            if (entity == null) return Result.Fail("Schedule not found");
            _context.DoctorSchedules.Remove(entity);
            await _context.SaveChangesAsync();
            return Result.Ok();
        }
    }
}