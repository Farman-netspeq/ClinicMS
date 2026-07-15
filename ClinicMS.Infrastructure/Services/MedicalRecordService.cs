using ClinicMS.Application.DTOs.MedicalRecord;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Domain.Enums;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicMS.Infrastructure.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalRecordService> _logger;

        public MedicalRecordService(ApplicationDbContext context, ILogger<MedicalRecordService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<MedicalRecordResponseDto>> GetByAppointmentIdAsync(string appointmentId)
        {
            try
            {
                var record = await _context.MedicalRecords
                    .FirstOrDefaultAsync(m => m.AppointmentId == appointmentId);

                if (record == null)
                    return Result<MedicalRecordResponseDto>.Fail("Medical record not found");

                return Result<MedicalRecordResponseDto>.Ok(new MedicalRecordResponseDto
                {
                    Id = record.Id,
                    AppointmentId = record.AppointmentId,
                    BloodPressure = record.BloodPressure,
                    Temperature = record.Temperature,
                    Pulse = record.Pulse,
                    Weight = record.Weight,
                    Height = record.Height,
                    Diagnosis = record.Diagnosis,
                    Notes = record.Notes,
                    CreatedOn = record.CreatedOn
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching medical record for appointment {AppointmentId}", appointmentId);
                return Result<MedicalRecordResponseDto>.Fail("Failed to fetch medical record");
            }
        }

        public async Task<Result<string>> CreateAsync(MedicalRecordRequestDto dto, string doctorUserId, bool isAdmin)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

                if (appointment == null)
                    return Result<string>.Fail("Appointment not found");

                // BR5 — status gate
                if (appointment.Status != AppointmentStatus.Completed)
                    return Result<string>.Fail("Medical record can only be created for a completed appointment");

                // BR5 — ownership gate (skip check entirely if Admin)
                if (!isAdmin)
                {
                    var doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.ApplicationUserId == doctorUserId);

                    if (doctor == null || doctor.Id != appointment.DoctorId)
                        return Result<string>.Fail("You are not authorized to record this consultation");
                }

                // BR5 — one record per appointment (also enforced by unique index, this is the friendly message)
                var exists = await _context.MedicalRecords
                    .AnyAsync(m => m.AppointmentId == dto.AppointmentId);

                if (exists)
                    return Result<string>.Fail("A medical record already exists for this appointment");

                var record = new MedicalRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    AppointmentId = dto.AppointmentId,
                    BloodPressure = dto.BloodPressure,
                    Temperature = dto.Temperature,
                    Pulse = dto.Pulse,
                    Weight = dto.Weight,
                    Height = dto.Height,
                    Diagnosis = dto.Diagnosis,
                    Notes = dto.Notes,
                    CreatedOn = DateTime.UtcNow,
                    CreatedById = doctorUserId
                };

                _context.MedicalRecords.Add(record);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Medical record created: {Id} for appointment {AppointmentId}", record.Id, record.AppointmentId);
                return Result<string>.Ok(record.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medical record: {@Dto}", dto);
                return Result<string>.Fail("Failed to create medical record");
            }
        }
    }
}