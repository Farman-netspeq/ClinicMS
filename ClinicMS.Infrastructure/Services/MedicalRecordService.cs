using ClinicMS.Application.DTOs.MedicalRecord;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Domain.Enums;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ClinicMS.Infrastructure.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<MedicalRecordService> _logger;

        public MedicalRecordService(ApplicationDbContext context, IAuditService auditService, ILogger<MedicalRecordService> logger)
        {
            _context = context;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<Result<MedicalRecordResponseDto>> GetByAppointmentIdAsync(string appointmentId)
        {
            try
            {
                var appointmentIdParam = new SqlParameter("@AppointmentId", appointmentId);

                var results = await _context.Set<MedicalRecordResponseDto>()
                    .FromSqlRaw("EXEC udspMedicalRecordsGetByAppointmentId @AppointmentId", appointmentIdParam)
                    .ToListAsync();

                var dto = results.FirstOrDefault();
                if (dto == null)
                    return Result<MedicalRecordResponseDto>.Fail("Medical record not found");

                return Result<MedicalRecordResponseDto>.Ok(dto);
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
                var id = Guid.NewGuid().ToString();

                var idParam = new SqlParameter("@Id", id);
                var appointmentIdParam = new SqlParameter("@AppointmentId", dto.AppointmentId);
                var bpParam = new SqlParameter("@BloodPressure", (object?)dto.BloodPressure ?? DBNull.Value);
                var tempParam = new SqlParameter("@Temperature", (object?)dto.Temperature ?? DBNull.Value);
                var pulseParam = new SqlParameter("@Pulse", (object?)dto.Pulse ?? DBNull.Value);
                var weightParam = new SqlParameter("@Weight", (object?)dto.Weight ?? DBNull.Value);
                var heightParam = new SqlParameter("@Height", (object?)dto.Height ?? DBNull.Value);
                var diagnosisParam = new SqlParameter("@Diagnosis", dto.Diagnosis.Trim());
                var notesParam = new SqlParameter("@Notes", (object?)dto.Notes?.Trim() ?? DBNull.Value);
                var lastUpdatedByParam = new SqlParameter("@LastUpdatedBy", doctorUserId);
                var isAdminParam = new SqlParameter("@IsAdmin", isAdmin);
                var resultParam = new SqlParameter
                {
                    ParameterName = "@Result",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 200,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspMedicalRecordsSave @Id, @AppointmentId, @BloodPressure, @Temperature, @Pulse, @Weight, @Height, @Diagnosis, @Notes, @LastUpdatedBy, @IsAdmin, @Result OUTPUT",
                    idParam, appointmentIdParam, bpParam, tempParam, pulseParam, weightParam, heightParam,
                    diagnosisParam, notesParam, lastUpdatedByParam, isAdminParam, resultParam);

                var errorMessage = resultParam.Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(errorMessage))
                    return Result<string>.Fail(errorMessage);

                _logger.LogInformation("Medical record created: {Id} for appointment {AppointmentId}", id, dto.AppointmentId);

                var userNameParam = new SqlParameter("@UserId", doctorUserId);
                var userNameResults = await _context.Database
                    .SqlQueryRaw<string>("EXEC udspUserGetUserName @UserId", userNameParam)
                    .ToListAsync();
                var userName = userNameResults.FirstOrDefault() ?? doctorUserId;

                await _auditService.LogAsync(doctorUserId, userName, AuditAction.Create, "MedicalRecord", id,
                    $"Diagnosis: {dto.Diagnosis.Trim()}");

                return Result<string>.Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medical record: {@Dto}", dto);
                return Result<string>.Fail("Failed to create medical record");
            }
        }
    }
}