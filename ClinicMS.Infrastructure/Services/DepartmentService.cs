using ClinicMS.Application.DTOs.Department;
using ClinicMS.Application.Interfaces;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicMS.Infrastructure.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(ApplicationDbContext context, ILogger<DepartmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── GET PAGED LIST ────────────────────────────────────
        public async Task<Result<PaginatedResult<DepartmentListItemDto>>>
            GetDepartmentsAsync(DepartmentFilterDto filter)
        {
            try
            {
                var searchTermParam = new SqlParameter("@SearchTerm", (object?)filter.SearchTerm ?? DBNull.Value);
                var isActiveParam = new SqlParameter("@IsActive", (object?)filter.IsActive ?? DBNull.Value);
                var pageNoParam = new SqlParameter("@PageNo", filter.PageNo);
                var pageSizeParam = new SqlParameter("@PageSize", filter.PageSize);

                var items = await _context.Set<DepartmentListItemDto>()
                    .FromSqlRaw(
                        "EXEC udspDeptPaged @SearchTerm, @IsActive, @PageNo, @PageSize",
                        searchTermParam, isActiveParam, pageNoParam, pageSizeParam)
                    .ToListAsync();

                var countResult = await _context.Set<DepartmentCountResultDto>()
                    .FromSqlRaw(
                        "EXEC udspDeptPagedCount @SearchTerm, @IsActive",
                        searchTermParam, isActiveParam)
                    .ToListAsync();

                var totalCount = countResult.FirstOrDefault()?.TotalCount ?? 0;

                var result = new PaginatedResult<DepartmentListItemDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = filter.PageNo,
                    PageSize = filter.PageSize
                };

                return Result<PaginatedResult<DepartmentListItemDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching departments. Filter: {@Filter}", filter);
                return Result<PaginatedResult<DepartmentListItemDto>>.Fail("Failed to fetch departments");
            }
        }

        // ── GET SINGLE BY ID ──────────────────────────────────
        public async Task<Result<DepartmentResponseDto>>
            GetDepartmentByIdAsync(string id)
        {
            try
            {
                var idParam = new SqlParameter("@Id", id);

                var results = await _context.Set<DepartmentResponseDto>()
                    .FromSqlRaw("EXEC udspDeptGetById @Id", idParam)
                    .ToListAsync();

                var dto = results.FirstOrDefault();

                if (dto == null)
                    return Result<DepartmentResponseDto>.Fail("Department not found");

                return Result<DepartmentResponseDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching department {DepartmentId}", id);
                return Result<DepartmentResponseDto>.Fail("Failed to fetch department");
            }
        }

        // ── CREATE ────────────────────────────────────────────
        public async Task<Result<string>>
            CreateDepartmentAsync(DepartmentRequestDto dto)
        {
            try
            {
                var nameParam = new SqlParameter("@Name", dto.Name.Trim());
                var descParam = new SqlParameter("@Description", (object?)dto.Description?.Trim() ?? DBNull.Value);

                var newIdParam = new SqlParameter
                {
                    ParameterName = "@NewId",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 450,
                    Direction = ParameterDirection.Output
                };

                var nameExistsParam = new SqlParameter
                {
                    ParameterName = "@NameExists",
                    SqlDbType = SqlDbType.Bit,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspDeptSave @Name, @Description, @NewId OUTPUT, @NameExists OUTPUT",
                    nameParam, descParam, newIdParam, nameExistsParam);

                var nameExists = (bool)(nameExistsParam.Value ?? false);
                if (nameExists)
                    return Result<string>.Fail($"Department '{dto.Name}' already exists");

                var newId = newIdParam.Value?.ToString() ?? string.Empty;

                _logger.LogInformation("Department created: {DepartmentId} - {Name}", newId, dto.Name);

                return Result<string>.Ok(newId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department: {@Dto}", dto);
                return Result<string>.Fail("Failed to create department");
            }
        }

        // ── UPDATE ────────────────────────────────────────────
        public async Task<Result>
            UpdateDepartmentAsync(DepartmentRequestDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Id))
                    return Result.Fail("Department Id is required");

                var idParam = new SqlParameter("@Id", dto.Id);
                var nameParam = new SqlParameter("@Name", dto.Name.Trim());
                var descParam = new SqlParameter("@Description", (object?)dto.Description?.Trim() ?? DBNull.Value);

                var nameExistsParam = new SqlParameter
                {
                    ParameterName = "@NameExists",
                    SqlDbType = SqlDbType.Bit,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspDeptUpdate @Id, @Name, @Description, @NameExists OUTPUT",
                    idParam, nameParam, descParam, nameExistsParam);

                var nameExists = (bool)(nameExistsParam.Value ?? false);
                if (nameExists)
                    return Result.Fail($"Department '{dto.Name}' already exists");

                _logger.LogInformation("Department updated: {DepartmentId}", dto.Id);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department {DepartmentId}", dto.Id);
                return Result.Fail("Failed to update department");
            }
        }

        // ── DEACTIVATE ────────────────────────────────────────
        public async Task<Result>
            DeactivateDepartmentAsync(string id)
        {
            try
            {
                var idParam = new SqlParameter("@Id", id);

                var hasActiveDoctorsParam = new SqlParameter
                {
                    ParameterName = "@HasActiveDoctors",
                    SqlDbType = SqlDbType.Bit,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC udspDeptDeactivate @Id, @HasActiveDoctors OUTPUT",
                    idParam, hasActiveDoctorsParam);

                var hasActiveDoctors = (bool)(hasActiveDoctorsParam.Value ?? false);
                if (hasActiveDoctors)
                    return Result.Fail(
                        "Cannot deactivate department with active doctors. " +
                        "Please reassign or deactivate doctors first.");

                _logger.LogInformation("Department deactivated: {DepartmentId}", id);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating department {DepartmentId}", id);
                return Result.Fail("Failed to deactivate department");
            }
        }

        // ── GET ACTIVE LIST (for dropdowns) ──────────────────
        public async Task<Result<List<DepartmentListItemDto>>>
            GetActiveDepartmentsAsync()
        {
            try
            {
                var departments = await _context.Set<DepartmentListItemDto>()
                    .FromSqlRaw("EXEC udspDeptActiveList")
                    .ToListAsync();

                return Result<List<DepartmentListItemDto>>.Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active departments");
                return Result<List<DepartmentListItemDto>>.Fail("Failed to fetch departments");
            }
        }
    }
}