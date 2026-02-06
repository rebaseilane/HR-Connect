namespace HRConnect.Api.Models
{
    
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Threading.Tasks;
    public class Employee
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Surname { get; set; } = string.Empty;
        [StringLength(13)]
        public int IDNumber { get; set; }
        public int PassportNumber { get; set; }
        [Required]
        public string Gender { get; set; } = string.Empty;
        [Required]
        [StringLength(10)]
        public int ContactNumber { get; set; }
        [Required]
        [EmailAddress]
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
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontlySalary { get; set; }
        [Required]
        public int  PositionId { get; set; }
        public Position? Position { get; set; }
        [Required]
        public string EmploymentStatus { get; set; } = string.Empty;
        [Required]
        public string CareerManger { get; set; } = string.Empty;
        [Required]
        public string EmpPicture { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        


    }
}