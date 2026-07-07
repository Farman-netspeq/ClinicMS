using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicMS.Application.DTOs.Patient
{
    public class PatientListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string PatientNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;   
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;     
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
