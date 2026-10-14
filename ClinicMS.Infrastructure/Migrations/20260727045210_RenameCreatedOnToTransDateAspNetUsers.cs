using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCreatedOnToTransDateAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppointmentCountResultDto",
                columns: table => new
                {
                    TotalCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "AppointmentListItemDto",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppointmentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "AppointmentResponseDto",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppointmentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentCountResultDto",
                columns: table => new
                {
                    TotalCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "DepartmentListItemDto",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "DepartmentResponseDto",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DoctorCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "DoctorCountResultDto",
                columns: table => new
                {
                    TotalCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "DoctorListItemDto",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "DoctorResponseDto",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "utblCMSRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utblCMSRefreshTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "utblCMSDepartments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utblCMSDepartments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "utblCMSPatients",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PatientNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    BloodGroup = table.Column<int>(type: "int", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utblCMSPatients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "utblCMSDoctors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DepartmentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utblCMSDoctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_utblCMSDoctors_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_utblCMSDoctors_utblCMSDepartments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "utblCMSDepartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "utblCMSAppointments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    AppointmentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DepartmentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "date", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utblCMSAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_utblCMSAppointments_AspNetUsers_LastUpdatedBy",
                        column: x => x.LastUpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_utblCMSAppointments_utblCMSDepartments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "utblCMSDepartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_utblCMSAppointments_utblCMSDoctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "utblCMSDoctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_utblCMSAppointments_utblCMSPatients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "utblCMSPatients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "utblCMSDoctorSchedules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    SlotDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utblCMSDoctorSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_utblCMSDoctorSchedules_utblCMSDoctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "utblCMSDoctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "utblCMSInvoices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    AppointmentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    IssuedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentMethod = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utblCMSInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_utblCMSInvoices_utblCMSAppointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "utblCMSAppointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_utblCMSInvoices_utblCMSPatients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "utblCMSPatients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "utblCMSMedicalRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    AppointmentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    BloodPressure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Temperature = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Pulse = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utblCMSMedicalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_utblCMSMedicalRecords_utblCMSAppointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "utblCMSAppointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "utblCMSPrescriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    AppointmentId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    IssuedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utblCMSPrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_utblCMSPrescriptions_utblCMSAppointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "utblCMSAppointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_utblCMSPrescriptions_utblCMSDoctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "utblCMSDoctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_utblCMSPrescriptions_utblCMSPatients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "utblCMSPatients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "utblCMSInvoiceItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    InvoiceId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utblCMSInvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_utblCMSInvoiceItems_utblCMSInvoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "utblCMSInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "utblCMSPrescriptionItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    PrescriptionId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    MedicationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utblCMSPrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_utblCMSPrescriptionItems_utblCMSPrescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "utblCMSPrescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSRefreshTokens_Token",
                table: "utblCMSRefreshTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSAppointments_AppointmentNumber",
                table: "utblCMSAppointments",
                column: "AppointmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSAppointments_DepartmentId",
                table: "utblCMSAppointments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSAppointments_DoctorId",
                table: "utblCMSAppointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSAppointments_LastUpdatedBy",
                table: "utblCMSAppointments",
                column: "LastUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSAppointments_PatientId",
                table: "utblCMSAppointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSDepartments_Name",
                table: "utblCMSDepartments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSDoctors_ApplicationUserId",
                table: "utblCMSDoctors",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSDoctors_DepartmentId",
                table: "utblCMSDoctors",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSDoctors_LicenseNumber",
                table: "utblCMSDoctors",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSDoctorSchedules_DoctorId_DayOfWeek",
                table: "utblCMSDoctorSchedules",
                columns: new[] { "DoctorId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSInvoiceItems_InvoiceId",
                table: "utblCMSInvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSInvoices_AppointmentId",
                table: "utblCMSInvoices",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSInvoices_InvoiceNumber",
                table: "utblCMSInvoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSInvoices_PatientId",
                table: "utblCMSInvoices",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSInvoices_Status",
                table: "utblCMSInvoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSMedicalRecords_AppointmentId",
                table: "utblCMSMedicalRecords",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSPatients_FirstName_LastName",
                table: "utblCMSPatients",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSPatients_PatientNumber",
                table: "utblCMSPatients",
                column: "PatientNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSPatients_Phone",
                table: "utblCMSPatients",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSPrescriptionItems_PrescriptionId",
                table: "utblCMSPrescriptionItems",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSPrescriptions_AppointmentId",
                table: "utblCMSPrescriptions",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSPrescriptions_DoctorId",
                table: "utblCMSPrescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSPrescriptions_PatientId",
                table: "utblCMSPrescriptions",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentCountResultDto");

            migrationBuilder.DropTable(
                name: "AppointmentListItemDto");

            migrationBuilder.DropTable(
                name: "AppointmentResponseDto");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DepartmentCountResultDto");

            migrationBuilder.DropTable(
                name: "DepartmentListItemDto");

            migrationBuilder.DropTable(
                name: "DepartmentResponseDto");

            migrationBuilder.DropTable(
                name: "DoctorCountResultDto");

            migrationBuilder.DropTable(
                name: "DoctorListItemDto");

            migrationBuilder.DropTable(
                name: "DoctorResponseDto");

            migrationBuilder.DropTable(
                name: "utblCMSRefreshTokens");

            migrationBuilder.DropTable(
                name: "utblCMSDoctorSchedules");

            migrationBuilder.DropTable(
                name: "utblCMSInvoiceItems");

            migrationBuilder.DropTable(
                name: "utblCMSMedicalRecords");

            migrationBuilder.DropTable(
                name: "utblCMSPrescriptionItems");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "utblCMSInvoices");

            migrationBuilder.DropTable(
                name: "utblCMSPrescriptions");

            migrationBuilder.DropTable(
                name: "utblCMSAppointments");

            migrationBuilder.DropTable(
                name: "utblCMSDoctors");

            migrationBuilder.DropTable(
                name: "utblCMSPatients");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "utblCMSDepartments");
        }
    }
}
