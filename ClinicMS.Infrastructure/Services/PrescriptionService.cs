using ClinicMS.Application.DTOs.Prescription;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Domain.Enums;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicMS.Infrastructure.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PrescriptionService> _logger;

        public PrescriptionService(ApplicationDbContext context, ILogger<PrescriptionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<PrescriptionResponseDto>> GetByAppointmentIdAsync(string appointmentId)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.Patient)
                    .Include(p => p.Doctor)
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

                if (prescription == null)
                    return Result<PrescriptionResponseDto>.Fail("Prescription not found");

                return Result<PrescriptionResponseDto>.Ok(new PrescriptionResponseDto
                {
                    Id = prescription.Id,
                    AppointmentId = prescription.AppointmentId,
                    PatientId = prescription.PatientId,
                    PatientName = $"{prescription.Patient?.FirstName} {prescription.Patient?.LastName}",
                    DoctorId = prescription.DoctorId,
                    DoctorName = prescription.Doctor?.FullName ?? string.Empty,
                    IssuedOn = prescription.IssuedOn,
                    Notes = prescription.Notes,
                    Items = prescription.Items.Select(i => new PrescriptionItemDto
                    {
                        Id = i.Id,
                        MedicationName = i.MedicationName,
                        Dosage = i.Dosage,
                        Frequency = i.Frequency,
                        DurationDays = i.DurationDays,
                        Instructions = i.Instructions
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching prescription for appointment {AppointmentId}", appointmentId);
                return Result<PrescriptionResponseDto>.Fail("Failed to fetch prescription");
            }
        }

        public async Task<Result<string>> CreateAsync(PrescriptionRequestDto dto, string doctorUserId, bool isAdmin)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

                if (appointment == null)
                    return Result<string>.Fail("Appointment not found");

                if (appointment.Status != AppointmentStatus.Completed)
                    return Result<string>.Fail("Prescription can only be created for a completed appointment");

                string effectiveDoctorId;
                if (!isAdmin)
                {
                    var doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.ApplicationUserId == doctorUserId);

                    if (doctor == null || doctor.Id != appointment.DoctorId)
                        return Result<string>.Fail("You are not authorized to prescribe for this consultation");

                    effectiveDoctorId = doctor.Id;
                }
                else
                {
                    effectiveDoctorId = appointment.DoctorId;   // Admin acts on behalf of the assigned doctor
                }

                var exists = await _context.Prescriptions
                    .AnyAsync(p => p.AppointmentId == dto.AppointmentId);

                if (exists)
                    return Result<string>.Fail("A prescription already exists for this appointment");

                if (dto.Items == null || dto.Items.Count == 0)
                    return Result<string>.Fail("At least one medication is required");

                var prescription = new Prescription
                {
                    Id = Guid.NewGuid().ToString(),
                    AppointmentId = dto.AppointmentId,
                    PatientId = appointment.PatientId,
                    DoctorId = effectiveDoctorId,
                    IssuedOn = DateTime.UtcNow,
                    Notes = dto.Notes,
                    Items = dto.Items.Select(i => new PrescriptionItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        MedicationName = i.MedicationName,
                        Dosage = i.Dosage,
                        Frequency = i.Frequency,
                        DurationDays = i.DurationDays,
                        Instructions = i.Instructions
                    }).ToList()
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Prescription created: {Id} for appointment {AppointmentId} with {Count} items",
                    prescription.Id, prescription.AppointmentId, prescription.Items.Count);

                return Result<string>.Ok(prescription.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription: {@Dto}", dto);
                return Result<string>.Fail("Failed to create prescription");
            }
        }
    }
}