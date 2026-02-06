namespace HRConnect.Api.DTOs.Employee
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    public class CreateEmployeeRequestDto
    {
        [Required, StringLength(50, MinimumLength = 1, ErrorMessage = "Employee name must be between 1 and 50 characters.")]
        public string Name { get; set; } = string.Empty;
        [Required, StringLength(100, MinimumLength = 1, ErrorMessage = "Employee surname must be between 1 and 100 characters.")]
        public string Surname { get; set; } = string.Empty;
        [StringLength(13)]
        public int IDNumber { get; set; }
        public int PassportNumber { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required, StringLength(10, ErrorMessage = "Contect Number must be 10 Digits long")]
        public int ContactNumber { get; set; }
        [Required, EmailAddress(ErrorMessage = "Email must end with 'singular.co.za'")]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PhysicalAddress { get; set; } = string.Empty;
        [Required]
        public DateOnly DateOfBirth { get; set; }
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public string Branch { get; set; } = string.Empty;
        [Required]
        public decimal MontlySalary { get; set; }
        [Required]
        public int  PositionId { get; set; }
        [Required]
        public string EmploymentStatus { get; set; } = string.Empty;
        [Required]
        public string CareerManger { get; set; } = string.Empty;
        [Required]
        public string EmpPicture { get; set; } = string.Empty;



        
    }
}