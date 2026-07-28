namespace ClinicMS.Application.DTOs.Prescription
{
    public class PrescriptionHeaderDto
    {
        public string Id { get; set; } = string.Empty;
        public string AppointmentId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime IssuedOn { get; set; }
        public string? Notes { get; set; }
    }
}