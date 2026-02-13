namespace HRConnect.Api.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs.Employee;
    using Microsoft.EntityFrameworkCore;
    /// <summary>
    /// Employee Repository file responsible for all Employee data access operations.
    /// Acts as a bridge between the database context and the service layer. - O.Seilane
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDBContext _context;
        public EmployeeRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Retrieves all Employees from the database.
        /// </summary>
        /// <returns> A List of all Employees</returns>
        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees.ToListAsync();
        }
        /// <summary>
        /// Creates a new Employee in the database.
        /// </summary>
        /// <param name="employeeModel">The employee model to create employee </param>
        /// <returns>The Created employee as objects, or null if inputs are invalid or duplicated</returns>
        public async Task<Employee> CreateEmployeeAsync(Employee employeeModel)
        {
            await _context.Employees.AddAsync(employeeModel);
            await _context.SaveChangesAsync();
            return employeeModel;
        }
        /// <summary>
        /// Creates a new Employee in the database.
        /// </summary>
        /// <param name="EmployeeId">The employee Id </param>
        /// <param name="EmployeeDto">The updated employee data </param>
        /// <returns>The Updated employee as objects, or null if inputs are invalid or duplicated</returns>
        public async Task<Employee?> UpdateEmployeeAsync(string EmployeeId, UpdateEmployeeRequestDto employeeDto)
        {
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == EmployeeId);
            
            if (existingEmployee == null)
            {
                return null;
            }

            existingEmployee.Title = employeeDto.Title;
            existingEmployee.Name = employeeDto.Name;
            existingEmployee.Surname = employeeDto.Surname;
            existingEmployee.IdNumber = employeeDto.IdNumber;
            existingEmployee.PassportNumber = employeeDto.PassportNumber;
            existingEmployee.ContactNumber = employeeDto.ContactNumber;
            existingEmployee.Email = employeeDto.Email;
            existingEmployee.PhysicalAddress = employeeDto.PhysicalAddress;
            existingEmployee.City = employeeDto.City;
            existingEmployee.ZipCode = employeeDto.ZipCode;
            existingEmployee.HasDisability = employeeDto.HasDisability;
            existingEmployee.DisabilityDescription = employeeDto.DisabilityDescription;
            existingEmployee.Branch = employeeDto.Branch;
            existingEmployee.MonthlySalary = employeeDto.MonthlySalary;
            existingEmployee.PositionId = employeeDto.PositionId;
            existingEmployee.EmploymentStatus = employeeDto.EmploymentStatus;
            existingEmployee.CareerManager = employeeDto.CareerManager;
            existingEmployee.EmpPicture = employeeDto.EmpPicture;
            existingEmployee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingEmployee;
        }
        /// <summary>
        /// Retrieves a single employee by their Employee Id.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier</param>
        /// <returns>The employee with the same Employee Id provided as EmployeeDTODto object or NotFound if the Employee Id does not exist</returns>
        public async Task<Employee?> GetEmployeeByIdAsync(string EmployeeId)
        {
            return await _context.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeId == EmployeeId);
        }
        /// <summary>
        /// Retrieves all employee IDs that start with a given prefix.
        /// Used for Employee ID auto-generation logic. 
        /// </summary>
        /// <param name="prefix">The employee prefix</param>
        /// <returns>All employees with the provided prefix</returns>
        public async Task<List<string>> GetAllEmployeeIdsWithPrefix(string prefix)
        {
            return await _context.Employees
                    .Where(e => e.EmployeeId.StartsWith(prefix))
                    .Select(e => e.EmployeeId)
                    .ToListAsync();
        }
        /// <summary>
        /// Deletes employee in the database by their Employee Id.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier</param>
        /// <returns>true; if employee exists, false; if employee doesn't exist</returns>
        public async Task<bool> DeleteEmployeeAsync(string EmployeeId)
        {
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == EmployeeId);

            if (existingEmployee == null)
                return false;

            _context.Employees.Remove(existingEmployee);
            await _context.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Checks if provided email already exist in the database
        /// </summary>
        /// <param name="Email">The employee email</param>
        /// <returns>True; if email already exist, False; if it doesn't exist</returns>
        public async Task<bool> EmailExistsAsync(string Email)
        {
            return await _context.Employees
                    .AnyAsync(e => e.Email == Email);
        }
        /// <summary>
        /// Checks if provided email already exist for other employees in the database
        /// </summary>
        /// <param name="EmployeeId">The employee identifier</param>
        /// <param name="Email">The employee email</param>
        /// <returns>True; If email already exists for another employee, False; if it doesn't exist or is the same employee</returns>
        public async Task<bool> EmailExistsForOthersAsync(string Email, string EmployeeId)
        {
            return await _context.Employees
                    .AnyAsync(e => e.Email == Email && e.EmployeeId != EmployeeId);
        }
        /// <summary>
        /// Checks whether a tax number already exists.
        /// </summary>
        /// <param name="TaxNumber">The employee tax number</param>
        /// <returns>True; if tax number already exists, False; if it doesn't exist</returns>
        public async Task<bool> TaxNumberExistsAsync(string TaxNumber)
        {
            return await _context.Employees
                    .AnyAsync(e => e.TaxNumber == TaxNumber);
        }
        /// <summary>
        /// Checks whether a Id number already exists.
        /// </summary>
        /// <param name="IdNumber">The employee ID number</param>
        /// <returns>True; if ID number already exists, False; if it doesn't exist</returns>
        public async Task<bool> IdNumberExistsAsync(string IdNumber)
        {
            return await _context.Employees
                    .AnyAsync(e => e.IdNumber == IdNumber);
        }
        /// <summary>
        /// Checks whether an ID number already exists for another employee.
        /// </summary>
        /// <param name="IdNumber">The employee ID number</param>
        /// <param name="EmployeeId">The employee identifier</param>
        /// <returns>True; if ID number already exists for another employee, False; if it doesn't exist or is the same employee</returns>
        public async Task<bool> IdNumberExistsForOthersAsync(string IdNumber, string EmployeeId)
        {
            return await _context.Employees
                    .AnyAsync(e => e.IdNumber == IdNumber && e.EmployeeId != EmployeeId);
        }
        /// <summary>
        /// Checks whether a passport number already exists.
        /// </summary>
        /// <param name="PassportNumber">The employee passport number</param>
        /// <returns>True; if passport number already exists, False; if it doesn't exist</returns>
        public async Task<bool> PassportExistsAsync(string PassportNumber)
        {
            return await _context.Employees
                    .AnyAsync(e => e.PassportNumber == PassportNumber);
        }
        /// <summary>
        /// Checks whether an Passport number already exists for another employee.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier</param>
        /// <param name="PassportNumber">The employee passport number</param>
        /// <returns>True; if passport number already exists for another employee, False; if it doesn't exist or is the same employee</returns>
        public async Task<bool> PassportExistsForOthersAsync(string PassportNumber, string EmployeeId)
        {
            return await _context.Employees
                    .AnyAsync(e => e.PassportNumber == PassportNumber && e.EmployeeId != EmployeeId);
        }
        /// <summary>
        /// Checks whether a contact number already exists.
        /// </summary>
        /// <param name="ContactNumber">The employee contact number</param>
        /// <returns>True; if contact number already exists, False; if it doesn't exist</returns>
        public async Task<bool> ContactExistsAsync(string ContactNumber)
        {
            return await _context.Employees
                    .AnyAsync(e => e.ContactNumber == ContactNumber);
        }
        /// <summary>
        /// Checks whether an contact number already exists for another employee.
        /// </summary>
        /// <param name="ContactNumber">The employee contact number</param>
        /// <param name="EmployeeId">The employee identifier</param>
        /// <returns>True; if contact number already exists for another employee, False; if it doesn't exist or is the same employee</returns>
        public async Task<bool> ContactExistsForOthersAsync(string ContactNumber, string EmployeeId)
        {
            return await _context.Employees
                    .AnyAsync(e => e.ContactNumber == ContactNumber && e.EmployeeId != EmployeeId);
        }
        
        
    }
}