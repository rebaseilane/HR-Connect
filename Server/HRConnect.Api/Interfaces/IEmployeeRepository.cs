namespace HRConnect.Api.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.Employee;
    
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(string EmployeeId);
        Task<Employee> CreateEmployeeAsync(Employee employeeModel);
        Task<Employee?> UpdateEmployeeAsync(string EmployeeId, UpdateEmployeeRequestDto employeeDto);
        Task<List<string>> GetAllEmployeeIdsWithPrefix(string prefix);
        Task<bool> DeleteEmployeeAsync(string EmployeeId);
        Task<bool> EmailExistsAsync(string Email);
        Task<bool> EmailExistsForOthersAsync(string Email, string EmployeeId);
        Task<bool> TaxNumberExistsAsync(string TaxNumber);
        Task<bool> IdNumberExistsAsync(string IdNumber);
        Task<bool> IdNumberExistsForOthersAsync(string IdNumber, string EmployeeId);
        Task<bool> PassportExistsAsync(string PassportNumber);
        Task<bool> PassportExistsForOthersAsync(string PassportNumber, string EmployeeId);
        Task<bool> ContactExistsAsync(string ContactNumber);
        Task<bool> ContactExistsForOthersAsync(string ContactNumber, string EmployeeId);

    }
}