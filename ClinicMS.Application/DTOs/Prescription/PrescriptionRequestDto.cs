using System.ComponentModel.DataAnnotations;

namespace ClinicMS.Application.DTOs.Prescription
{
    public class PrescriptionRequestDto
    {
        [Required]
        public string AppointmentId { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "At least one medication is required")]
        [MinLength(1, ErrorMessage = "Add at least one medication")]
        public List<PrescriptionItemDto> Items { get; set; } = new();
    }
}