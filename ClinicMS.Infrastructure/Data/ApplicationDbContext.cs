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


    public DbSet<utblCMSRefreshToken> RefreshTokens { get; set; }
    public DbSet<utblCMSDepartments> Departments { get; set; }
    public DbSet<utblCMSDoctors> Doctors { get; set; }
    public DbSet<utblCMSDoctorSchedules> DoctorSchedules { get; set; }
    public DbSet<utblCMSPatients> Patients { get; set; }
    public DbSet<utblCMSAppointments> Appointments { get; set; }
    public DbSet<utblCMSMedicalRecords> MedicalRecords { get; set; }
    public DbSet<utblCMSPrescriptions> Prescriptions { get; set; }
    public DbSet<utblCMSPrescriptionItems> PrescriptionItems { get; set; }
    public DbSet<utblCMSInvoices> Invoices { get; set; }
    public DbSet<utblCMSInvoiceItems> InvoiceItems { get; set; }
    public DbSet<utblCMSAuditLogs> AuditLogs { get; set; }
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
        modelBuilder.Entity<DepartmentListItemDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DepartmentCountResultDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DepartmentResponseDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DoctorListItemDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DoctorCountResultDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DoctorResponseDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DoctorScheduleListItemDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DoctorScheduleResponseDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<AppointmentListItemDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<AppointmentCountResultDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<AppointmentResponseDto>().HasNoKey().ToView(null);
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
        modelBuilder.Entity<AuditLogListItemDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<AuditLogCountResultDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DashboardSummaryDto>().HasNoKey().ToView(null);
    }
}