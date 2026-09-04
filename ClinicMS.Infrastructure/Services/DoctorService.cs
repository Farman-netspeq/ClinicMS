using ClinicMS.Application.DTOs.Doctor;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicMS.Infrastructure.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DoctorService> _logger;

        public DoctorService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<DoctorService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // ── GET PAGED LIST ────────────────────────────────────
        public async Task<Result<PaginatedResult<DoctorListItemDto>>>
            GetDoctorsAsync(DoctorFilterDto filter)
        {
            try
            {
                var searchTermParam = new SqlParameter("@SearchTerm", (object?)filter.SearchTerm ?? DBNull.Value);
                var departmentIdParam = new SqlParameter("@DepartmentId", (object?)filter.DepartmentId ?? DBNull.Value);
                var isActiveParam = new SqlParameter("@IsActive", (object?)filter.IsActive ?? DBNull.Value);
                var pageNoParam = new SqlParameter("@PageNo", filter.PageNo);
                var pageSizeParam = new SqlParameter("@PageSize", filter.PageSize);

                var items = await _context.Set<DoctorListItemDto>()
                    .FromSqlRaw(
                        "EXEC udspDoctorsPaged @SearchTerm, @DepartmentId, @IsActive, @PageNo, @PageSize",
                        searchTermParam, departmentIdParam, isActiveParam, pageNoParam, pageSizeParam)
                    .ToListAsync();

                // fresh params for count — same SqlParameter instance can't run in two EXEC calls
                var countSearchTermParam = new SqlParameter("@SearchTerm", (object?)filter.SearchTerm ?? DBNull.Value);
                var countDepartmentIdParam = new SqlParameter("@DepartmentId", (object?)filter.DepartmentId ?? DBNull.Value);
                var countIsActiveParam = new SqlParameter("@IsActive", (object?)filter.IsActive ?? DBNull.Value);

                var countResult = await _context.Set<DoctorCountResultDto>()
                    .FromSqlRaw(
                        "EXEC udspDoctorsPagedCount @SearchTerm, @DepartmentId, @IsActive",
                        countSearchTermParam, countDepartmentIdParam, countIsActiveParam)
                    .ToListAsync();

                var totalCount = countResult.FirstOrDefault()?.TotalCount ?? 0;

                var result = new PaginatedResult<DoctorListItemDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = filter.PageNo,
                    PageSize = filter.PageSize
                };

                return Result<PaginatedResult<DoctorListItemDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctors. Filter: {@Filter}", filter);
                return Result<PaginatedResult<DoctorListItemDto>>.Fail("Failed to fetch doctors");
            }
        }

        // ── GET SINGLE BY ID ──────────────────────────────────
        public async Task<Result<DoctorResponseDto>>
            GetDoctorByIdAsync(string id)
        {
            try
            {
                var idParam = new SqlParameter("@Id", id);

                var results = await _context.Set<DoctorResponseDto>()
                    .FromSqlRaw("EXEC udspDoctorsGetById @Id", idParam)
                    .ToListAsync();

                var dto = results.FirstOrDefault();

                if (dto == null)
                    return Result<DoctorResponseDto>.Fail("Doctor not found");

                return Result<DoctorResponseDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctor {DoctorId}", id);
                return Result<DoctorResponseDto>.Fail("Failed to fetch doctor");
            }
        }

        // ── CREATE ────────────────────────────────────────────
        public async Task<Result<string>>
            CreateDoctorAsync(DoctorRequestDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Email))
                    return Result<string>.Fail("Email is required for doctor login");
                if (string.IsNullOrEmpty(dto.Password))
                    return Result<string>.Fail("Password is required for doctor login");

                var licenseExists = await EnsureUniqueLicenseAsync(dto.LicenseNumber, excludeId: null);
                if (licenseExists)
                    return Result<string>.Fail($"License '{dto.LicenseNumber}' already exists");

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var userResult = await _userManager.CreateAsync(user, dto.Password);

                if (!userResult.Succeeded)
                {
                    var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                    return Result<string>.Fail(errors);
                }

                await _userManager.AddToRoleAsync(user, "Doctor");

                var userIdParam = new SqlParameter("@ApplicationUserId", user.Id);
                var fullNameParam = new SqlParameter("@FullName", dto.FullName.Trim());
                var departmentIdParam = new SqlParameter("@DepartmentId", dto.DepartmentId);
                var specializationParam = new SqlParameter("@Specialization", dto.Specialization.Trim());
                var licenseNumberParam = new SqlParameter("@LicenseNumber", dto.LicenseNumber.Trim());
                var feeParam = new SqlParameter("@ConsultationFee", dto.ConsultationFee);

                var newIdParam = new SqlParameter
                {
                    ParameterName = "@NewId",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 36,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspDoctorsSave @ApplicationUserId, @FullName, @DepartmentId, @Specialization, @LicenseNumber, @ConsultationFee, @NewId OUTPUT",
                    userIdParam, fullNameParam, departmentIdParam, specializationParam, licenseNumberParam, feeParam, newIdParam);

                var newId = newIdParam.Value?.ToString() ?? string.Empty;

                _logger.LogInformation("Doctor created: {DoctorId} - {Name} with user {UserId}", newId, dto.FullName, user.Id);

                return Result<string>.Ok(newId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating doctor: {@Dto}", dto);
                return Result<string>.Fail("Failed to create doctor");
            }
        }

        // ── UPDATE ────────────────────────────────────────────
        public async Task<Result>
            UpdateDoctorAsync(DoctorRequestDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Id))
                    return Result.Fail("Doctor Id is required");

                var licenseExists = await EnsureUniqueLicenseAsync(dto.LicenseNumber, excludeId: dto.Id);
                if (licenseExists)
                    return Result.Fail($"License '{dto.LicenseNumber}' already exists");

                var idParam = new SqlParameter("@Id", dto.Id);
                var fullNameParam = new SqlParameter("@FullName", dto.FullName.Trim());
                var departmentIdParam = new SqlParameter("@DepartmentId", dto.DepartmentId);
                var specializationParam = new SqlParameter("@Specialization", dto.Specialization.Trim());
                var licenseNumberParam = new SqlParameter("@LicenseNumber", dto.LicenseNumber.Trim());
                var feeParam = new SqlParameter("@ConsultationFee", dto.ConsultationFee);

                var userIdParam = new SqlParameter
                {
                    ParameterName = "@ApplicationUserId",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 450,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspDoctorsUpdate @Id, @FullName, @DepartmentId, @Specialization, @LicenseNumber, @ConsultationFee, @ApplicationUserId OUTPUT",
                    idParam, fullNameParam, departmentIdParam, specializationParam, licenseNumberParam, feeParam, userIdParam);

                // keep navbar name synced with ApplicationUser
                var userId = userIdParam.Value?.ToString();
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        user.FullName = dto.FullName.Trim();
                        await _userManager.UpdateAsync(user);
                    }
                }

                _logger.LogInformation("Doctor updated: {DoctorId}", dto.Id);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor {DoctorId}", dto.Id);
                return Result.Fail("Failed to update doctor");
            }
        }

        // ── DEACTIVATE ────────────────────────────────────────
        public async Task<Result>
            DeactivateDoctorAsync(string id)
            => await SetActiveAsync(id, isActive: false);

        // ── REACTIVATE ────────────────────────────────────────
        public async Task<Result>
            ReactivateDoctorAsync(string id)
            => await SetActiveAsync(id, isActive: true);

        private async Task<Result>
            SetActiveAsync(string id, bool isActive)
        {
            try
            {
                var idParam = new SqlParameter("@Id", id);
                var isActiveParam = new SqlParameter("@IsActive", isActive);

                var userIdParam = new SqlParameter
                {
                    ParameterName = "@ApplicationUserId",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 450,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspDoctorsSetActive @Id, @IsActive, @ApplicationUserId OUTPUT",
                    idParam, isActiveParam, userIdParam);

                var userId = userIdParam.Value?.ToString();
                if (string.IsNullOrEmpty(userId))
                    return Result.Fail("Doctor not found");

                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.IsActive = isActive;
                    await _userManager.UpdateAsync(user);
                }

                _logger.LogInformation("Doctor {State}: {DoctorId}", isActive ? "reactivated" : "deactivated", id);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting doctor active state {DoctorId}", id);
                return Result.Fail("Failed to update doctor status");
            }
        }

        // ── GET BY DEPARTMENT (cascading dropdown) ────────────
        public async Task<Result<List<DoctorListItemDto>>>
            GetDoctorsByDepartmentAsync(string departmentId)
        {
            try
            {
                var departmentIdParam = new SqlParameter("@DepartmentId", SqlDbType.UniqueIdentifier)
                {
                    Value = Guid.Parse(departmentId)
                };

                var doctors = await _context.Set<DoctorListItemDto>()
                    .FromSqlRaw("EXEC udspDoctorsByDepartment @DepartmentId", departmentIdParam)
                    .ToListAsync();

                return Result<List<DoctorListItemDto>>.Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctors for dept {DeptId}", departmentId);
                return Result<List<DoctorListItemDto>>.Fail("Failed to fetch doctors");
            }
        }

        // ── PRIVATE HELPERS ───────────────────────────────────
        private async Task<bool>
            EnsureUniqueLicenseAsync(string licenseNumber, string? excludeId)
        {
            var licenseNumberParam = new SqlParameter("@LicenseNumber", licenseNumber);
            var excludeIdParam = new SqlParameter("@ExcludeId", (object?)excludeId ?? DBNull.Value);

            var existsParam = new SqlParameter
            {
                ParameterName = "@Exists",
                SqlDbType = SqlDbType.Bit,
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC udspDoctorsCheckLicense @LicenseNumber, @ExcludeId, @Exists OUTPUT",
                licenseNumberParam, excludeIdParam, existsParam);

            return (bool)(existsParam.Value ?? false);
        }
        public async Task<Result<string>> GetDoctorIdByUserIdAsync(string userId)
        {
            try
            {
                var userIdParam = new SqlParameter("@ApplicationUserId", userId);
                var results = await _context.Set<DoctorResponseDto>()
                    .FromSqlRaw("EXEC udspDoctorsGetByUserId @ApplicationUserId", userIdParam)
                    .ToListAsync();
                var dto = results.FirstOrDefault();
                return dto == null ? Result<string>.Fail("Doctor not found") : Result<string>.Ok(dto.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving doctor for user {UserId}", userId);
                return Result<string>.Fail("Failed to resolve doctor");
            }
        }
    }
}