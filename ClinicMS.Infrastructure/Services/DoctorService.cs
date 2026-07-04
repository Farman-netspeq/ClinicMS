using ClinicMS.Application.DTOs.Doctor;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicMS.Infrastructure.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        // UserManager = Identity service for creating users
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
                var query = _context.Doctors
                    .Include(d => d.Department)
                    // Include Department → get dept name for grid
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    query = query.Where(d =>
                        d.FullName.Contains(filter.SearchTerm) ||
                        d.LicenseNumber.Contains(filter.SearchTerm) ||
                        d.Specialization.Contains(filter.SearchTerm));

                if (!string.IsNullOrWhiteSpace(filter.DepartmentId))
                    query = query.Where(d =>
                        d.DepartmentId == filter.DepartmentId);

                if (filter.IsActive.HasValue)
                    query = query.Where(d =>
                        d.IsActive == filter.IsActive.Value);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(d => d.FullName)
                    .Skip((filter.PageNo - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(d => new DoctorListItemDto
                    {
                        Id = d.Id,
                        FullName = d.FullName,
                        DepartmentName = d.Department != null
                            ? d.Department.Name : "N/A",
                        Specialization = d.Specialization,
                        LicenseNumber = d.LicenseNumber,
                        ConsultationFee = d.ConsultationFee,
                        IsActive = d.IsActive
                    })
                    .ToListAsync();

                return Result<PaginatedResult<DoctorListItemDto>>.Ok(
                    new PaginatedResult<DoctorListItemDto>
                    {
                        Items = items,
                        TotalCount = totalCount,
                        Page = filter.PageNo,
                        PageSize = filter.PageSize
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctors");
                return Result<PaginatedResult<DoctorListItemDto>>
                    .Fail("Failed to fetch doctors");
            }
        }

        // ── GET BY ID ─────────────────────────────────────────
        public async Task<Result<DoctorResponseDto>>
            GetDoctorByIdAsync(string id)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.Department)
                    .Include(d => d.ApplicationUser)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (doctor == null)
                    return Result<DoctorResponseDto>
                        .Fail("Doctor not found");

                return Result<DoctorResponseDto>.Ok(
                    new DoctorResponseDto
                    {
                        Id = doctor.Id,
                        FullName = doctor.FullName,
                        DepartmentId = doctor.DepartmentId,
                        DepartmentName = doctor.Department?.Name
                            ?? "N/A",
                        Specialization = doctor.Specialization,
                        LicenseNumber = doctor.LicenseNumber,
                        ConsultationFee = doctor.ConsultationFee,
                        Email = doctor.ApplicationUser?.Email
                            ?? string.Empty,
                        IsActive = doctor.IsActive,
                        ApplicationUserId = doctor.ApplicationUserId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching doctor {DoctorId}", id);
                return Result<DoctorResponseDto>
                    .Fail("Failed to fetch doctor");
            }
        }

        // ── CREATE ────────────────────────────────────────────
        // 1. ApplicationUser (login account)
        // 2. Doctor entity (medical profile)
        public async Task<Result<string>>
            CreateDoctorAsync(DoctorRequestDto dto)
        {
            try
            {
                // Validate required create fields
                if (string.IsNullOrEmpty(dto.Email))
                    return Result<string>
                        .Fail("Email is required for doctor login");
                if (string.IsNullOrEmpty(dto.Password))
                    return Result<string>
                        .Fail("Password is required for doctor login");

                // license must be unique
                var licenseExists = await EnsureUniqueLicenseAsync(
                    dto.LicenseNumber, excludeId: null);
                if (licenseExists)
                    return Result<string>
                        .Fail($"License '{dto.LicenseNumber}' already exists");

                // Create ApplicationUser (login account)
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var userResult = await _userManager
                    .CreateAsync(user, dto.Password);
                // CreateAsync = hashes password + saves to AspNetUsers

                if (!userResult.Succeeded)
                {
                    var errors = string.Join(", ",
                        userResult.Errors.Select(e => e.Description));
                    return Result<string>.Fail(errors);
                }

                // Step 2: Assign Doctor role to the user
                await _userManager.AddToRoleAsync(user, "Doctor");

                // Step 3: Create Doctor entity linking to user
                var doctor = new Doctor
                {
                    Id = Guid.NewGuid().ToString(),
                    ApplicationUserId = user.Id,
                    // Links doctor profile to login account
                    FullName = dto.FullName.Trim(),
                    DepartmentId = dto.DepartmentId,
                    Specialization = dto.Specialization.Trim(),
                    LicenseNumber = dto.LicenseNumber.Trim(),
                    ConsultationFee = dto.ConsultationFee,
                    IsActive = true
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Doctor created: {DoctorId} - {Name} with user {UserId}",
                    doctor.Id, doctor.FullName, user.Id);

                return Result<string>.Ok(doctor.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating doctor: {@Dto}", dto);
                return Result<string>
                    .Fail("Failed to create doctor");
            }
        }

        // ── UPDATE ────────────────────────────────────────────
        // Updates Doctor entity only
        public async Task<Result> UpdateDoctorAsync(DoctorRequestDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Id))
                    return Result.Fail("Doctor Id required");

                var doctor = await _context.Doctors
                    .FindAsync(dto.Id);

                if (doctor == null)
                    return Result.Fail("Doctor not found");

                var licenseExists = await EnsureUniqueLicenseAsync(
                    dto.LicenseNumber, excludeId: dto.Id);
                if (licenseExists)
                    return Result.Fail(
                        $"License '{dto.LicenseNumber}' already exists");

                // Update doctor fields
                doctor.FullName = dto.FullName.Trim();
                doctor.DepartmentId = dto.DepartmentId;
                doctor.Specialization = dto.Specialization.Trim();
                doctor.LicenseNumber = dto.LicenseNumber.Trim();
                doctor.ConsultationFee = dto.ConsultationFee;

                // Also update FullName in ApplicationUser
                // So navbar shows correct name after edit
                var user = await _userManager
                    .FindByIdAsync(doctor.ApplicationUserId);
                if (user != null)
                {
                    user.FullName = dto.FullName.Trim();
                    await _userManager.UpdateAsync(user);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Doctor updated: {DoctorId}", dto.Id);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating doctor {DoctorId}", dto.Id);
                return Result.Fail("Failed to update doctor");
            }
        }

        // ── DEACTIVATE ────────────────────────────────────────
        public async Task<Result> DeactivateDoctorAsync(string id)
        {
            try
            {
                var doctor = await _context.Doctors
                    .FindAsync(id);

                if (doctor == null)
                    return Result.Fail("Doctor not found");

                // Deactivate both doctor profile AND login account
                doctor.IsActive = false;

                var user = await _userManager
                    .FindByIdAsync(doctor.ApplicationUserId);
                if (user != null)
                {
                    user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Doctor deactivated: {DoctorId}", id);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deactivating doctor {DoctorId}", id);
                return Result.Fail("Failed to deactivate doctor");
            }
        }

        // ── REACTIVATE ────────────────────────────────────────
        public async Task<Result> ReactivateDoctorAsync(string id)
        {
            try
            {
                var doctor = await _context.Doctors
                    .FindAsync(id);

                if (doctor == null)
                    return Result.Fail("Doctor not found");

                doctor.IsActive = true;

                var user = await _userManager
                    .FindByIdAsync(doctor.ApplicationUserId);
                if (user != null)
                {
                    user.IsActive = true;
                    await _userManager.UpdateAsync(user);
                }

                await _context.SaveChangesAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error reactivating doctor {DoctorId}", id);
                return Result.Fail("Failed to reactivate doctor");
            }
        }

        // ── GET BY DEPARTMENT (cascading dropdown) ────────────
        public async Task<Result<List<DoctorListItemDto>>>
            GetDoctorsByDepartmentAsync(string departmentId)
        {
            try
            {
                var doctors = await _context.Doctors
                    .Where(d => d.DepartmentId == departmentId
                        && d.IsActive)
                    .OrderBy(d => d.FullName)
                    .Select(d => new DoctorListItemDto
                    {
                        Id = d.Id,
                        FullName = d.FullName,
                        Specialization = d.Specialization,
                        IsActive = d.IsActive
                    })
                    .ToListAsync();

                return Result<List<DoctorListItemDto>>.Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching doctors for dept {DeptId}",
                    departmentId);
                return Result<List<DoctorListItemDto>>
                    .Fail("Failed to fetch doctors");
            }
        }

        // ── PRIVATE HELPERS ───────────────────────────────────
        private async Task<bool> EnsureUniqueLicenseAsync(
            string licenseNumber, string? excludeId)
        {
            return await _context.Doctors
                .AnyAsync(d =>
                    d.LicenseNumber.ToLower()
                        == licenseNumber.ToLower() &&
                    d.Id != excludeId);
        }
    }
}