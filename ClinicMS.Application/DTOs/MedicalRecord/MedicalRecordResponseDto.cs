namespace ClinicMS.Application.DTOs.MedicalRecord
{
    public class MedicalRecordResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string AppointmentId { get; set; } = string.Empty;
        public string? BloodPressure { get; set; }
        public decimal? Temperature { get; set; }
        public int? Pulse { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime TransDate { get; set; }
    }
}