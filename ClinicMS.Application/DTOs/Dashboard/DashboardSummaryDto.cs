namespace ClinicMS.Application.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TodaysAppointments { get; set; }
        public int PatientsSeenToday { get; set; }
        public int PendingInvoices { get; set; }
        public decimal RevenueThisMonth { get; set; }
    }
}