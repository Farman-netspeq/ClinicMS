using ClinicMS.Domain.Enums;

namespace ClinicMS.Domain.Entities
{
    public class Invoice
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string AppointmentId { get; set; } = string.Empty;
        public DateTime IssuedOn { get; set; } = DateTime.UtcNow;
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
        public decimal TotalAmount { get; set; }
        public DateTime? PaidOn { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        public Patient? Patient { get; set; }
        public Appointment? Appointment { get; set; }
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }
}