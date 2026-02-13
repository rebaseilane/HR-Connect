namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.Employee;
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(string EmployeeId);
        Task<Employee> CreateEmployeeAsync(CreateEmployeeRequestDto employeeRequestDto);
        Task<Employee?> UpdateEmployeeAsync(string EmployeeId, UpdateEmployeeRequestDto employeeDto);
        Task<bool> DeleteEmployeeAsync(string EmployeeId);

    }
}