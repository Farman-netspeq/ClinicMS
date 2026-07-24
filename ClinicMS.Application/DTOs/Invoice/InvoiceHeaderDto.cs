using ClinicMS.Domain.Enums;

namespace ClinicMS.Application.DTOs.Invoice
{
    public class InvoiceHeaderDto
    {
        public string Id { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string AppointmentId { get; set; } = string.Empty;
        public DateTime IssuedOn { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? PaidOn { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
    }
}