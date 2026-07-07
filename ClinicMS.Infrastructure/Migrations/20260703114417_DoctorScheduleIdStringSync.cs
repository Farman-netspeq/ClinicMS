using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DoctorScheduleIdStringSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Already dropped in prior partial run — skip
            // migrationBuilder.DropForeignKey(...)
            // migrationBuilder.DropIndex(name: "IX_utblCMSDoctorSchedules_DoctorId_DayOfWeek", ...)

            migrationBuilder.AlterColumn<string>(
                name: "DoctorId",
                table: "utblCMSDoctorSchedules",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "utblCMSDoctorSchedules",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSDoctorSchedules_DoctorId_DayOfWeek",
                table: "utblCMSDoctorSchedules",
                columns: new[] { "DoctorId", "DayOfWeek" });

            migrationBuilder.AddForeignKey(
                name: "FK_utblCMSDoctorSchedules_utblCMSDoctors_DoctorId",
                table: "utblCMSDoctorSchedules",
                column: "DoctorId",
                principalTable: "utblCMSDoctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_utblCMSDoctorSchedules_DoctorId",
                table: "utblCMSDoctorSchedules");

            migrationBuilder.AlterColumn<string>(
                name: "DoctorId",
                table: "utblCMSDoctorSchedules",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "utblCMSDoctorSchedules",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_utblCMSDoctorSchedules_DoctorId_DayOfWeek",
                table: "utblCMSDoctorSchedules",
                columns: new[] { "DoctorId", "DayOfWeek" },
                unique: true);
        }
    }
}
