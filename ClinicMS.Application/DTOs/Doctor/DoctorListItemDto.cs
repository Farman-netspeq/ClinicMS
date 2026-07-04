namespace ClinicMS.Application.DTOs.Doctor
{
    public class DoctorListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public bool IsActive { get; set; }
        public string StatusDisplay => IsActive ? "Active" : "Inactive";
    }
}