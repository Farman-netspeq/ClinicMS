using ClinicMS.Application.DTOs.Patient;
using ClinicMS.Application.Interfaces;
using ClinicMS.Domain.Entities;
using ClinicMS.Infrastructure.Data;
using ClinicMS.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        // ── GET PAGED LIST (search by name/phone/patient number) ──
        public async Task<Result<PaginatedResult<PatientListItemDto>>> GetPatientsAsync(PatientFilterDto filter)
        {
            try
            {
                var query = _context.Patients.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var term = filter.SearchTerm.Trim();
                    query = query.Where(p =>
                        p.FirstName.Contains(term) ||
                        p.LastName.Contains(term) ||
                        p.Phone.Contains(term) ||
                        p.PatientNumber.Contains(term));
                }

                if (filter.IsActive.HasValue)
                    query = query.Where(p => p.IsActive == filter.IsActive.Value);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
                    .Skip((filter.PageNo - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(p => new PatientListItemDto
                    {
                        Id = p.Id,
                        PatientNumber = p.PatientNumber,
                        FullName = p.FirstName + " " + p.LastName,
                        Age = DateTime.Today.Year - p.DateOfBirth.Year -
                              (DateTime.Today.DayOfYear < p.DateOfBirth.DayOfYear ? 1 : 0),
                        Gender = p.Gender.ToString(),
                        Phone = p.Phone,
                        IsActive = p.IsActive
                    })
                    .ToListAsync();

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
                var p = await _context.Patients.FindAsync(id);
                if (p == null) return Result<PatientResponseDto>.Fail("Patient not found");

                var dto = new PatientResponseDto
                {
                    Id = p.Id,
                    PatientNumber = p.PatientNumber,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    BloodGroup = p.BloodGroup,
                    Phone = p.Phone,
                    Email = p.Email,
                    Address = p.Address,
                    IsActive = p.IsActive
                };
                return Result<PatientResponseDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching patient {Id}", id);
                return Result<PatientResponseDto>.Fail("Failed to fetch patient");
            }
        }

        // ── CREATE──
        public async Task<Result<string>> CreatePatientAsync(PatientRequestDto dto)
        {
            try
            {
                if (dto.DateOfBirth > DateTime.Today)
                    return Result<string>.Fail("Date of birth cannot be in the future");

                var patientNumber = await GeneratePatientNumberAsync();

                var patient = new Patient
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientNumber = patientNumber,
                    FirstName = dto.FirstName.Trim(),
                    LastName = dto.LastName.Trim(),
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    BloodGroup = dto.BloodGroup,
                    Phone = dto.Phone.Trim(),
                    Email = dto.Email?.Trim(),
                    Address = dto.Address?.Trim(),
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Patient created: {Id} - {PatientNumber}", patient.Id, patient.PatientNumber);
                return Result<string>.Ok(patient.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient: {@Dto}", dto);
                return Result<string>.Fail("Failed to create patient");
            }
        }

        // ── PatientNumber generator (BR1) ──
        private async Task<string> GeneratePatientNumberAsync()
        {
            var lastPatient = await _context.Patients
                .OrderByDescending(p => p.PatientNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastPatient != null)
            {
                var numericPart = lastPatient.PatientNumber.Replace("PAT-", "");
                if (int.TryParse(numericPart, out int lastNumber))
                    nextNumber = lastNumber + 1;
            }

            return $"PAT-{nextNumber:D6}";
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

                var patient = await _context.Patients.FindAsync(dto.Id);
                if (patient == null) return Result.Fail("Patient not found");

                // PatientNumber never updated — immutable once assigned
                patient.FirstName = dto.FirstName.Trim();
                patient.LastName = dto.LastName.Trim();
                patient.DateOfBirth = dto.DateOfBirth;
                patient.Gender = dto.Gender;
                patient.BloodGroup = dto.BloodGroup;
                patient.Phone = dto.Phone.Trim();
                patient.Email = dto.Email?.Trim();
                patient.Address = dto.Address?.Trim();

                await _context.SaveChangesAsync();
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
            try
            {
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null) return Result.Fail("Patient not found");

                if (!patient.IsActive)
                    return Result.Fail("Patient is already inactive");

                patient.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Patient deactivated: {Id}", id);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating patient {Id}", id);
                return Result.Fail("Failed to deactivate patient");
            }
        }

        public async Task<Result> ReactivatePatientAsync(string id)
        {
            try
            {
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null) return Result.Fail("Patient not found");

                if (patient.IsActive)
                    return Result.Fail("Patient is already active");

                patient.IsActive = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Patient reactivated: {Id}", id);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating patient {Id}", id);
                return Result.Fail("Failed to reactivate patient");
            }
        }
    }
}