using ClinicMS.Application.DTOs.Appointment;
using ClinicMS.Application.DTOs.Audit;
using ClinicMS.Application.DTOs.Dashboard;
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
    public DbSet<AuditLog> AuditLogs { get; set; }
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
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.Entity<DepartmentListItemDto>().HasNoKey();
        modelBuilder.Entity<DepartmentCountResultDto>().HasNoKey();
        modelBuilder.Entity<DepartmentResponseDto>().HasNoKey();
        modelBuilder.Entity<DoctorListItemDto>().HasNoKey();
        modelBuilder.Entity<DoctorCountResultDto>().HasNoKey();
        modelBuilder.Entity<DoctorResponseDto>().HasNoKey();
        modelBuilder.Entity<DoctorScheduleListItemDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DoctorScheduleResponseDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<AppointmentListItemDto>().HasNoKey();
        modelBuilder.Entity<AppointmentCountResultDto>().HasNoKey();
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
        modelBuilder.Entity<AuditLogListItemDto>().HasNoKey();
        modelBuilder.Entity<AuditLogCountResultDto>().HasNoKey();
        modelBuilder.Entity<DashboardSummaryDto>().HasNoKey();
    }
}