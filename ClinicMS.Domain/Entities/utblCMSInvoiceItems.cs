namespace ClinicMS.Domain.Entities
{
    public class utblCMSInvoiceItems
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string InvoiceId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }

        public utblCMSInvoices? Invoice { get; set; }
    }
}