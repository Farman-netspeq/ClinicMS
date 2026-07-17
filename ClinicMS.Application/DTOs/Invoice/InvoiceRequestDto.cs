using System.ComponentModel.DataAnnotations;

namespace ClinicMS.Application.DTOs.Invoice
{
    public class InvoiceRequestDto
    {
        [Required]
        public string AppointmentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one line item is required")]
        [MinLength(1, ErrorMessage = "Add at least one line item")]
        public List<InvoiceItemDto> Items { get; set; } = new();
    }
}