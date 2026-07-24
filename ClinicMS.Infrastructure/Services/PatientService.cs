using ClinicMS.Application.DTOs.Patient;
using ClinicMS.Application.Interfaces;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicMS.Infrastructure.Services
{
    public class PatientService : IPatientService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PatientService> _logger;

        public PatientService(ApplicationDbContext context, ILogger<PatientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── GET PAGED LIST ──
        public async Task<Result<PaginatedResult<PatientListItemDto>>> GetPatientsAsync(PatientFilterDto filter)
        {
            try
            {
                var searchTermParam = new SqlParameter("@SearchTerm", (object?)filter.SearchTerm ?? DBNull.Value);
                var isActiveParam = new SqlParameter("@IsActive", (object?)filter.IsActive ?? DBNull.Value);
                var pageNoParam = new SqlParameter("@PageNo", filter.PageNo);
                var pageSizeParam = new SqlParameter("@PageSize", filter.PageSize);

                var items = await _context.PatientListItems
                .FromSqlRaw("EXEC udspPatientsPaged @SearchTerm, @IsActive, @PageNo, @PageSize",
                     searchTermParam, isActiveParam, pageNoParam, pageSizeParam)
                    .AsNoTracking()
                     .ToListAsync();

                var countSearchTermParam = new SqlParameter("@SearchTerm", (object?)filter.SearchTerm ?? DBNull.Value);
                var countIsActiveParam = new SqlParameter("@IsActive", (object?)filter.IsActive ?? DBNull.Value);

                var countResult = (await _context.PatientCounts
               .FromSqlRaw("EXEC udspPatientsPagedCount @SearchTerm, @IsActive", countSearchTermParam, countIsActiveParam)
               .ToListAsync())
                .FirstOrDefault();

                var totalCount = countResult?.TotalCount ?? 0;

                var result = new PaginatedResult<PatientListItemDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = filter.PageNo,
                    PageSize = filter.PageSize
                };

                return Result<PaginatedResult<PatientListItemDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching patients. Filter: {@Filter}", filter);
                return Result<PaginatedResult<PatientListItemDto>>.Fail("Failed to fetch patients");
            }
        }

        // ── GET SINGLE BY ID ──
        public async Task<Result<PatientResponseDto>> GetPatientByIdAsync(string id)
        {
            try
            {
                var idParam = new SqlParameter("@Id", id);

                var results = await _context.PatientResponses
                    .FromSqlRaw("EXEC udspPatientsGetById @Id", idParam)
                    .ToListAsync();

                var dto = results.FirstOrDefault();
                if (dto == null)
                    return Result<PatientResponseDto>.Fail("Patient not found");

                return Result<PatientResponseDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching patient {Id}", id);
                return Result<PatientResponseDto>.Fail("Failed to fetch patient");
            }
        }

        // ── CREATE (BR1 handled inside SP) ──
        public async Task<Result<string>> CreatePatientAsync(PatientRequestDto dto)
        {
            try
            {
                if (dto.DateOfBirth > DateTime.Today)
                    return Result<string>.Fail("Date of birth cannot be in the future");

                var id = Guid.NewGuid().ToString();

                var idParam = new SqlParameter("@Id", id);
                var firstNameParam = new SqlParameter("@FirstName", dto.FirstName.Trim());
                var lastNameParam = new SqlParameter("@LastName", dto.LastName.Trim());
                var dobParam = new SqlParameter("@DateOfBirth", dto.DateOfBirth);
                var genderParam = new SqlParameter("@Gender", (int)dto.Gender);
                var bloodGroupParam = new SqlParameter("@BloodGroup", (int)dto.BloodGroup);
                var phoneParam = new SqlParameter("@Phone", dto.Phone.Trim());
                var emailParam = new SqlParameter("@Email", (object?)dto.Email?.Trim() ?? DBNull.Value);
                var addressParam = new SqlParameter("@Address", (object?)dto.Address?.Trim() ?? DBNull.Value);
                var newPatientNumberParam = new SqlParameter
                {
                    ParameterName = "@NewPatientNumber",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 20,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspPatientsSave @Id, @FirstName, @LastName, @DateOfBirth, @Gender, @BloodGroup, @Phone, @Email, @Address, @NewPatientNumber OUTPUT",
                    idParam, firstNameParam, lastNameParam, dobParam, genderParam, bloodGroupParam,
                    phoneParam, emailParam, addressParam, newPatientNumberParam);

                var patientNumber = newPatientNumberParam.Value?.ToString() ?? string.Empty;

                _logger.LogInformation("Patient created: {Id} - {PatientNumber}", id, patientNumber);
                return Result<string>.Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient: {@Dto}", dto);
                return Result<string>.Fail("Failed to create patient");
            }
        }

        // ── UPDATE ──
        public async Task<Result> UpdatePatientAsync(PatientRequestDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Id))
                    return Result.Fail("Patient Id is required");

                if (dto.DateOfBirth > DateTime.Today)
                    return Result.Fail("Date of birth cannot be in the future");

                var idParam = new SqlParameter("@Id", dto.Id);
                var firstNameParam = new SqlParameter("@FirstName", dto.FirstName.Trim());
                var lastNameParam = new SqlParameter("@LastName", dto.LastName.Trim());
                var dobParam = new SqlParameter("@DateOfBirth", dto.DateOfBirth);
                var genderParam = new SqlParameter("@Gender", (int)dto.Gender);
                var bloodGroupParam = new SqlParameter("@BloodGroup", (int)dto.BloodGroup);
                var phoneParam = new SqlParameter("@Phone", dto.Phone.Trim());
                var emailParam = new SqlParameter("@Email", (object?)dto.Email?.Trim() ?? DBNull.Value);
                var addressParam = new SqlParameter("@Address", (object?)dto.Address?.Trim() ?? DBNull.Value);

                var rows = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspPatientsUpdate @Id, @FirstName, @LastName, @DateOfBirth, @Gender, @BloodGroup, @Phone, @Email, @Address",
                    idParam, firstNameParam, lastNameParam, dobParam, genderParam, bloodGroupParam,
                    phoneParam, emailParam, addressParam);

                if (rows == 0)
                    return Result.Fail("Patient not found");

                _logger.LogInformation("Patient updated: {Id}", dto.Id);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient {Id}", dto.Id);
                return Result.Fail("Failed to update patient");
            }
        }

        // ── SOFT DELETE (BR7) ──
        public async Task<Result> DeactivatePatientAsync(string id)
        {
            return await SetActiveAsync(id, false, "deactivat");
        }

        // ── REACTIVATE ──
        public async Task<Result> ReactivatePatientAsync(string id)
        {
            return await SetActiveAsync(id, true, "reactivat");
        }

        // ── PRIVATE HELPER — shared by Deactivate/Reactivate ──
        private async Task<Result> SetActiveAsync(string id, bool isActive, string actionVerb)
        {
            try
            {
                var idParam = new SqlParameter("@Id", id);
                var isActiveParam = new SqlParameter("@IsActive", isActive);
                var resultParam = new SqlParameter
                {
                    ParameterName = "@Result",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 200,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspPatientsSetActive @Id, @IsActive, @Result OUTPUT",
                    idParam, isActiveParam, resultParam);

                var errorMessage = resultParam.Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(errorMessage))
                    return Result.Fail(errorMessage);

                _logger.LogInformation("Patient {Verb}ed: {Id}", actionVerb, id);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error {Verb}ing patient {Id}", actionVerb, id);
                return Result.Fail($"Failed to {actionVerb}e patient");
            }
        }
    }
}