using System.ComponentModel.DataAnnotations;

namespace ClinicMS.Application.DTOs.Prescription
{
    public class PrescriptionItemDto
    {
        public string? Id { get; set; }   // null for new items, populated for existing on edit

        [Required(ErrorMessage = "Medication name is required")]
        [StringLength(200)]
        public string MedicationName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Dosage { get; set; }

        [StringLength(100)]
        public string? Frequency { get; set; }

        [Range(1, 365)]
        public int? DurationDays { get; set; }

        [StringLength(500)]
        public string? Instructions { get; set; }
    }
}