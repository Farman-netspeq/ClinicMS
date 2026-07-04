using System.Numerics;

namespace ClinicMS.Domain.Entities
{
    // Maps to utblCMSDepartments table (set in EF config)
    // One department has many doctors
    public class Department
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation property — EF uses this to load related doctors
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}