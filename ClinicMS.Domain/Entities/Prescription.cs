namespace ClinicMS.Domain.Entities
{
    public class Prescription
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AppointmentId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public DateTime IssuedOn { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public Appointment? Appointment { get; set; }
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
    }
}