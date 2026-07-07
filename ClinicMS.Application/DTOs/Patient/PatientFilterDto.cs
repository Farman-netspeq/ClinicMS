using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicMS.Application.DTOs.Patient
{
    public class PatientFilterDto
    {
        public string? SearchTerm { get; set; }   
        public bool? IsActive { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
