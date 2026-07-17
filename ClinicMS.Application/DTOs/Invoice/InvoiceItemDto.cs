using System.ComponentModel.DataAnnotations;

namespace ClinicMS.Application.DTOs.Invoice
{
    public class InvoiceItemDto
    {
        public string? Id { get; set; }   

        [Required(ErrorMessage = "Description is required")]
        [StringLength(300)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Range(0, 1000000, ErrorMessage = "Unit price must be 0 or greater")]
        public decimal UnitPrice { get; set; }
    }
}