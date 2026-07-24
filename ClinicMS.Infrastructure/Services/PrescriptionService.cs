using System.Data;
using System.Text.Json;
using ClinicMS.Application.DTOs.Prescription;
using ClinicMS.Application.Interfaces;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.Data.SqlClient;
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
                var appointmentIdParam = new SqlParameter("@AppointmentId", appointmentId);
                var header = (await _context.PrescriptionHeaders
                    .FromSqlRaw("EXEC udspPrescriptionsGetByAppointmentId @AppointmentId", appointmentIdParam)
                    .ToListAsync())
                    .FirstOrDefault();

                if (header == null)
                    return Result<PrescriptionResponseDto>.Fail("Prescription not found");

                var prescriptionIdParam = new SqlParameter("@PrescriptionId", header.Id);
                var items = await _context.PrescriptionItemResults
                    .FromSqlRaw("EXEC udspPrescriptionItemsGetByPrescriptionId @PrescriptionId", prescriptionIdParam)
                    .ToListAsync();

                return Result<PrescriptionResponseDto>.Ok(new PrescriptionResponseDto
                {
                    Id = header.Id,
                    AppointmentId = header.AppointmentId,
                    PatientId = header.PatientId,
                    PatientName = header.PatientName,
                    DoctorId = header.DoctorId,
                    DoctorName = header.DoctorName,
                    IssuedOn = header.IssuedOn,
                    Notes = header.Notes,
                    Items = items
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
                var id = Guid.NewGuid().ToString();
                var itemsJson = JsonSerializer.Serialize(dto.Items);

                var idParam = new SqlParameter("@Id", id);
                var appointmentIdParam = new SqlParameter("@AppointmentId", dto.AppointmentId);
                var notesParam = new SqlParameter("@Notes", (object?)dto.Notes?.Trim() ?? DBNull.Value);
                var doctorUserIdParam = new SqlParameter("@DoctorUserId", doctorUserId);
                var isAdminParam = new SqlParameter("@IsAdmin", isAdmin);
                var itemsJsonParam = new SqlParameter("@ItemsJson", itemsJson);
                var resultParam = new SqlParameter
                {
                    ParameterName = "@Result",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 200,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspPrescriptionsSave @Id, @AppointmentId, @Notes, @DoctorUserId, @IsAdmin, @ItemsJson, @Result OUTPUT",
                    idParam, appointmentIdParam, notesParam, doctorUserIdParam, isAdminParam, itemsJsonParam, resultParam);

                var errorMessage = resultParam.Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(errorMessage))
                    return Result<string>.Fail(errorMessage);

                _logger.LogInformation("Prescription created: {Id} for appointment {AppointmentId} with {Count} items",
                    id, dto.AppointmentId, dto.Items.Count);

                return Result<string>.Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription: {@Dto}", dto);
                return Result<string>.Fail("Failed to create prescription");
            }
        }
    }
}