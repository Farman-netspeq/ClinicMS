using System.ComponentModel.DataAnnotations;

namespace ClinicMS.Application.DTOs.MedicalRecord
{
    public class MedicalRecordRequestDto
    {
        [Required]
        public string AppointmentId { get; set; } = string.Empty;

        [StringLength(20)]
        public string? BloodPressure { get; set; }

        [Range(30, 45, ErrorMessage = "Enter a valid temperature in Celsius")]
        public decimal? Temperature { get; set; }

        [Range(30, 250, ErrorMessage = "Enter a valid pulse rate")]
        public int? Pulse { get; set; }

        [Range(1, 300, ErrorMessage = "Enter a valid weight in kg")]
        public decimal? Weight { get; set; }

        [Range(30, 250, ErrorMessage = "Enter a valid height in cm")]
        public decimal? Height { get; set; }

        [Required(ErrorMessage = "Diagnosis is required")]
        [StringLength(1000)]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Notes { get; set; }
    }
}