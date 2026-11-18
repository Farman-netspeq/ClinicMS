using ClinicMS.Application.DTOs.Appointment;
using ClinicMS.Application.DTOs.Prescription;

namespace ClinicMS.Application.DTOs.MedicalRecord
{
    public class ConsultationViewDto
    {
        public AppointmentResponseDto Appointment { get; set; } = default!;
        public MedicalRecordResponseDto MedicalRecord { get; set; } = default!;
        public PrescriptionResponseDto Prescription { get; set; } = default!;
    }
}
