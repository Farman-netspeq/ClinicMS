namespace ClinicMS.Application.DTOs.Doctor
{
    public class DoctorResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string DepartmentId { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
    }
}