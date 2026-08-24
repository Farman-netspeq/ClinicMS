namespace ClinicMS.Domain.Entities
{
    public class utblCMSPrescriptions
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AppointmentId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public DateTime IssuedOn { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public utblCMSAppointments? Appointment { get; set; }
        public utblCMSPatients? Patient { get; set; }
        public utblCMSDoctors? Doctor { get; set; }
        public ICollection<utblCMSPrescriptionItems> Items { get; set; } = new List<utblCMSPrescriptionItems>();
    }
}