using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRConnect.Api.DTOs.Employee
{
    public class UpdateEmployeeRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhysicalAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public bool HasDisability { get; set; }
        public string? DisabilityDescription { get; set; }
        public string Branch { get; set; } = string.Empty;
        public decimal MonthlySalary { get; set; }       
        public int  PositionId { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;
        public string CareerManager { get; set; } = string.Empty;
        public string EmpPicture { get; set; } = string.Empty;
    }
}