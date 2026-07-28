namespace ClinicMS.Domain.Entities
{
    public class MedicalRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AppointmentId { get; set; } = string.Empty;
        public string? BloodPressure { get; set; }
        public decimal? Temperature { get; set; }
        public int? Pulse { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime TransDate { get; set; } = DateTime.UtcNow;
        public string? LastUpdatedBy { get; set; }

        public Appointment? Appointment { get; set; }
    }
}