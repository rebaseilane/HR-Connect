namespace HRConnect.Api.Mappers
{
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
    public static class EmployeeMapper
    {
        public static EmployeeDto ToEmployeeDto(this Employee employeeModel)
        {
            return new EmployeeDto
            {
                EmployeeId = employeeModel.EmployeeId,
                Name = employeeModel.Name,
                Surname = employeeModel.Surname,
                IDNumber = employeeModel.IDNumber,
                PassportNumber = employeeModel.PassportNumber,
                Gender = employeeModel.Gender,
                ContactNumber = employeeModel.ContactNumber,
                Email = employeeModel.Email,
                PhysicalAddress = employeeModel.PhysicalAddress,
                DateOfBirth = employeeModel.DateOfBirth,
                StartDate = employeeModel.StartDate,
                Branch = employeeModel.Banch,
                MonthlySalary = employeeModel.MonthlySalary,
                PositionId = employeeModel.PositionId,
                CareerManager = employeeModel.CareerManger,
                EmpPicture = employeeModel.EmPicture,
                CreatedAt = employeeModel.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = employeeModel.UpdatedAt ?? DateTime.UtcNow
            };
        }


        public static Employee ToEmployeeFromCreateDTO(this CreateEmployeeRequestDto employeeRequestDto)
        {
            return new Employee
            {
                Name = employeeRequestDto.Name,
                Surname = employeeRequestDto.Surname,
                IDNumber = employeeRequestDto.IDNumber,
                PassportNumber = employeeRequestDto.PassportNumber,
                Gender = employeeRequestDto.Gender,
                ContactNumber = employeeRequestDto.ContactNumber,
                Email = employeeRequestDto.Email,
                PhysicalAddress = employeeRequestDto.PhysicalAddress,
                DateOfBirth = employeeRequestDto.DateOfBirth,
                StartDate = employeeRequestDto.StartDate,
                Branch = employeeRequestDto.Branch,
                MontlySalary = employeeRequestDto.MontlySalary,
                PositionId = employeeRequestDto.PositionId,
                EmploymentStatus = employeeRequestDto.EmploymentStatus,
                CareerManger = employeeRequestDto.CareerManger,
                EmpPicture = employeeRequestDto.EmpPicture
            };
        }    
    }
}