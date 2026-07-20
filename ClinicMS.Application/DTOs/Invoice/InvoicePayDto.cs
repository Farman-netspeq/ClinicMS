using System.ComponentModel.DataAnnotations;
using ClinicMS.Domain.Enums;

namespace ClinicMS.Application.DTOs.Invoice
{
    public class InvoicePayDto
    {
        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethod PaymentMethod { get; set; }
    }
}