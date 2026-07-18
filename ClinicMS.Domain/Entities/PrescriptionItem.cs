namespace ClinicMS.Domain.Entities
{
    public class PrescriptionItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PrescriptionId { get; set; } = string.Empty;
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public int? DurationDays { get; set; }
        public string? Instructions { get; set; }

        public Prescription? Prescription { get; set; }
    }
}