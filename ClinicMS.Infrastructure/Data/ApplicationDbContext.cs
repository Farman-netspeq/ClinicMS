using ClinicMS.Application.DTOs.Appointment;
using ClinicMS.Application.DTOs.Department;
using ClinicMS.Application.DTOs.Doctor;
using ClinicMS.Application.DTOs.DoctorSchedule;
using ClinicMS.Application.DTOs.Invoice;
using ClinicMS.Application.DTOs.MedicalRecord;
using ClinicMS.Application.DTOs.Patient;
using ClinicMS.Application.DTOs.Prescription;
using ClinicMS.Domain.Entities;
using ClinicMS.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicMS.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }


    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }

    public DbSet<DepartmentListItemDto> DepartmentListItems { get; set; }
    public DbSet<DepartmentCountResult> DepartmentCounts { get; set; }
    public DbSet<DepartmentResponseDto> DepartmentResponses { get; set; }

    public DbSet<DoctorListItemDto> DoctorListItems { get; set; }
    public DbSet<DoctorCountResultDto> DoctorCounts { get; set; }
    public DbSet<DoctorResponseDto> DoctorResponses { get; set; }
    public DbSet<DoctorScheduleListItemDto> DoctorScheduleListItems { get; set; }
    public DbSet<DoctorScheduleResponseDto> DoctorScheduleResponses { get; set; }

    public DbSet<AppointmentListItemDto> AppointmentListItems { get; set; }
    public DbSet<AppointmentCountResult> AppointmentCounts { get; set; }
    public DbSet<AppointmentResponseDto> AppointmentResponses { get; set; }

    public DbSet<PatientListItemDto> PatientListItems { get; set; }
    public DbSet<PatientResponseDto> PatientResponses { get; set; }
    public DbSet<PatientCountResultDto> PatientCounts { get; set; }
    public DbSet<MedicalRecordResponseDto> MedicalRecordResponses { get; set; }
    public DbSet<InvoiceListItemDto> InvoiceListItems { get; set; }
    public DbSet<InvoiceCountResultDto> InvoiceCounts { get; set; }
    public DbSet<InvoiceHeaderDto> InvoiceHeaders { get; set; }
    public DbSet<InvoiceItemDto> InvoiceItemResults { get; set; }
    public DbSet<PrescriptionHeaderDto> PrescriptionHeaders { get; set; }
    public DbSet<PrescriptionItemDto> PrescriptionItemResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new DoctorConfiguration());
        modelBuilder.ApplyConfiguration(new DoctorScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new PatientConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalRecordConfiguration());
        modelBuilder.ApplyConfiguration(new PrescriptionConfiguration());
        modelBuilder.ApplyConfiguration(new PrescriptionItemConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceItemConfiguration());
        modelBuilder.Entity<DepartmentListItemDto>().HasNoKey();
        modelBuilder.Entity<DepartmentCountResult>().HasNoKey();
        modelBuilder.Entity<DepartmentResponseDto>().HasNoKey();
        modelBuilder.Entity<DoctorListItemDto>().HasNoKey();
        modelBuilder.Entity<DoctorCountResultDto>().HasNoKey();
        modelBuilder.Entity<DoctorResponseDto>().HasNoKey();
        modelBuilder.Entity<DoctorScheduleListItemDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DoctorScheduleResponseDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<AppointmentListItemDto>().HasNoKey();
        modelBuilder.Entity<AppointmentCountResult>().HasNoKey();
        modelBuilder.Entity<AppointmentResponseDto>().HasNoKey();
        modelBuilder.Entity<PatientListItemDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<PatientResponseDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<PatientCountResultDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<MedicalRecordResponseDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<InvoiceListItemDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<InvoiceCountResultDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<InvoiceHeaderDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<InvoiceItemDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<PrescriptionHeaderDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<PrescriptionItemDto>().HasNoKey().ToView(null);
    }
}