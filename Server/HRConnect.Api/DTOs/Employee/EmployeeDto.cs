namespace HRConnect.Api.DTOs.Employee
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public int IDNumber { get; set; }
        public int PassportNumber { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int ContactNumber { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhysicalAddress { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public DateOnly StartDate { get; set; }
        public string Branch { get; set; } = string.Empty;
        public decimal MonthlySalary { get; set; }
        public int  PositionId { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;
        public string CareerManger { get; set; } = string.Empty;
        public string EmpPicture { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}