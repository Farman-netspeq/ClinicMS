using System.ComponentModel.DataAnnotations;

namespace ClinicMS.Application.DTOs.Appointment
{
    public class AppointmentRequestDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Patient is required")]
        public string PatientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public string DepartmentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doctor is required")]
        public string DoctorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Appointment date is required")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Time slot is required")]
        public string StartTime { get; set; } = string.Empty;

        public string? EndTime { get; set; }

        [Required(ErrorMessage = "Chief complaint is required")]
        [StringLength(500, ErrorMessage = "Cannot exceed 500 characters")]
        public string ChiefComplaint { get; set; } = string.Empty;

        public string? CancelReason { get; set; }
    }
}