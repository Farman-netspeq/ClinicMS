using ClinicMS.Application.DTOs.Department;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicMS.Infrastructure.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentService> _logger;
        // ILogger = built-in .NET logging
        // Logs errors with context so you can debug issues

        public DepartmentService(
            ApplicationDbContext context,
            ILogger<DepartmentService> logger)
        {
            _context = context;
            _logger = logger;
            // Both injected by DI automatically
        }

        // ── GET PAGED LIST ────────────────────────────────────
        public async Task<Result<PaginatedResult<DepartmentListItemDto>>>
            GetDepartmentsAsync(DepartmentFilterDto filter)
        {
            try
            {
                // Just builds the SQL, doesn't hit DB yet
                var query = _context.Departments.AsQueryable();

                // Apply search filter if provided
                // EF translates this to: WHERE Name LIKE '%cardio%'
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    query = query.Where(d =>
                        d.Name.Contains(filter.SearchTerm));
                }

                // Apply active/inactive filter if provided
                if (filter.IsActive.HasValue)
                {
                    query = query.Where(d =>
                        d.IsActive == filter.IsActive.Value);
                }

                // Count BEFORE paging — gives total for "50 results"
                // This hits DB: SELECT COUNT(*) WHERE filters
                var totalCount = await query.CountAsync();

                // Apply paging — SKIP rows before current page
                // Take only PageSize rows
                // Page 1: Skip(0).Take(10) = rows 1-10
                var items = await query
                    .OrderBy(d => d.Name)
                    .Skip((filter.PageNo - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(d => new DepartmentListItemDto
                    // Select = manual mapping Entity → DTO
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        IsActive = d.IsActive
                    })
                    .ToListAsync();

                var result = new PaginatedResult<DepartmentListItemDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = filter.PageNo,
                    PageSize = filter.PageSize
                };

                return Result<PaginatedResult<DepartmentListItemDto>>
                    .Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching departments. Filter: {@Filter}",
                    filter);
                return Result<PaginatedResult<DepartmentListItemDto>>
                    .Fail("Failed to fetch departments");
            }
        }

        // ── GET SINGLE BY ID ──────────────────────────────────
        public async Task<Result<DepartmentResponseDto>>
            GetDepartmentByIdAsync(string id)
        {
            try
            {
                var department = await _context.Departments
                    .Include(d => d.Doctors)
                    // Include = JOIN with Doctors table
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (department == null)
                    return Result<DepartmentResponseDto>
                        .Fail("Department not found");

                // Manual mapping — entity → ResponseDto
                var dto = new DepartmentResponseDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    IsActive = department.IsActive,
                    DoctorCount = department.Doctors
                        .Count(d => d.IsActive)
                };

                return Result<DepartmentResponseDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching department {DepartmentId}", id);
                return Result<DepartmentResponseDto>
                    .Fail("Failed to fetch department");
            }
        }

        // ── CREATE ────────────────────────────────────────────
        public async Task<Result<string>>
            CreateDepartmentAsync(DepartmentRequestDto dto)
        {
            try
            {
                // Business rule: Name must be unique
                var nameExists = await EnsureUniqueNameAsync(
                    dto.Name, excludeId: null);
                if (nameExists)
                    return Result<string>
                        .Fail($"Department '{dto.Name}' already exists");

                // Manual mapping — DTO → Entity
                var department = new Department
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name.Trim(),
                    Description = dto.Description?.Trim()
                        ?? string.Empty,
                    IsActive = true
                };

                _context.Departments.Add(department);

                await _context.SaveChangesAsync();
                // NOW saves to DB — INSERT SQL runs here

                _logger.LogInformation(
                    "Department created: {DepartmentId} - {Name}",
                    department.Id, department.Name);

                return Result<string>.Ok(department.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating department: {@Dto}", dto);
                return Result<string>
                    .Fail("Failed to create department");
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

                // Find existing entity
                var department = await _context.Departments
                    .FindAsync(dto.Id);

                if (department == null)
                    return Result.Fail("Department not found");

                // Business rule: new name must still be unique
                // excludeId = this dept's own Id so it doesn't
                var nameExists = await EnsureUniqueNameAsync(
                    dto.Name, excludeId: dto.Id);
                if (nameExists)
                    return Result.Fail(
                        $"Department '{dto.Name}' already exists");

                // Update only changed fields — manual mapping
                department.Name = dto.Name.Trim();
                department.Description = dto.Description?.Trim()
                    ?? string.Empty;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Department updated: {DepartmentId}", dto.Id);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating department {DepartmentId}", dto.Id);
                return Result.Fail("Failed to update department");
            }
        }

        // ── DEACTIVATE ────────────────────────────────────────
        public async Task<Result>
            DeactivateDepartmentAsync(string id)
        {
            try
            {
                var department = await _context.Departments
                    .Include(d => d.Doctors)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (department == null)
                    return Result.Fail("Department not found");
                var hasActiveDoctors = department.Doctors
                    .Any(d => d.IsActive);
                if (hasActiveDoctors)
                    return Result.Fail(
                        "Cannot deactivate department with active doctors. " +
                        "Please reassign or deactivate doctors first.");

                department.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Department deactivated: {DepartmentId}", id);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deactivating department {DepartmentId}", id);
                return Result.Fail("Failed to deactivate department");
            }
        }

        // ── GET ACTIVE LIST (for dropdowns) ──────────────────
        public async Task<Result<List<DepartmentListItemDto>>>
            GetActiveDepartmentsAsync()
        {
            try
            {
                var departments = await _context.Departments
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Name)
                    .Select(d => new DepartmentListItemDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        IsActive = d.IsActive
                    })
                    .ToListAsync();

                return Result<List<DepartmentListItemDto>>
                    .Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching active departments");
                return Result<List<DepartmentListItemDto>>
                    .Fail("Failed to fetch departments");
            }
        }

        // ── PRIVATE HELPERS ───────────────────────────────────
        // Small named method = business rule obvious in code=
        private async Task<bool> EnsureUniqueNameAsync(
            string name, string? excludeId)
        {
            // Check if any OTHER department has same name
            // excludeId = skip checking against itself (for update)
            return await _context.Departments
                .AnyAsync(d =>
                    d.Name.ToLower() == name.ToLower() &&
                    d.Id != excludeId);
            // ToLower comparison = case-insensitive
            // "cardiology" == "Cardiology" == "CARDIOLOGY"
        }
    }
}