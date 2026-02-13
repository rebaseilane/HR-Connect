namespace HRConnect.Tests
{
    using Xunit;
    using Moq;
    using HRConnect.Api.Services;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.Employee;
    using HRConnect.Api.Utils;
    using HRConnect.Api.Mappers;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Net.NetworkInformation;

    public class EmployeeServiceTests
    {
        private readonly Mock<IEmployeeRepository> _employeeRepoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly EmployeeService _employeeService;


        public EmployeeServiceTests()
        {
            _employeeRepoMock = new Mock<IEmployeeRepository>();
            _emailServiceMock = new Mock<IEmailService>();

            _employeeService = new EmployeeService(
                _employeeRepoMock.Object,
                _emailServiceMock.Object
            );
        }


        [Fact]
        public async Task CreateEmployeeAsyncValidInputReturnsCreatedEmployee()
        {
            // Arrange
            var employeeDto = new CreateEmployeeRequestDto
            {
                Name = "John",
                Surname = "Smith",
                Title = "Mr",
                Gender = "Male",
                IdNumber = "0305055487589",
                TaxNumber = "1234567890",
                PhysicalAddress = "123 Main St",
                Email = "john.smith@singular.co.za",
                ContactNumber = "0123456789",
                Branch = "Johannesburg",
                City = "Johannesburg",
                ZipCode = "2000",
                PositionId = 1,
                MonthlySalary = 20000,
                EmploymentStatus = "Permanent",
                CareerManager = "Manager Name",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EmpPicture = "profile.jpg"
            };

            var createdEmployee = employeeDto.ToEmployeeFromCreateDTO();

            _employeeRepoMock.Setup(r => r.CreateEmployeeAsync(It.IsAny<Employee>()))
                             .ReturnsAsync(createdEmployee);

            _employeeRepoMock.Setup(r => r.GetAllEmployeeIdsWithPrefix(It.IsAny<string>()))
                             .ReturnsAsync(new List<string>());

            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .Returns(Task.CompletedTask);

            _employeeRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                            .ReturnsAsync(false);
            _employeeRepoMock.Setup(r => r.TaxNumberExistsAsync(It.IsAny<string>()))
                            .ReturnsAsync(false);
            _employeeRepoMock.Setup(r => r.IdNumberExistsAsync(It.IsAny<string>()))
                            .ReturnsAsync(false);
            _employeeRepoMock.Setup(r => r.PassportExistsAsync(It.IsAny<string>()))
                            .ReturnsAsync(false);
            _employeeRepoMock.Setup(r => r.ContactExistsAsync(It.IsAny<string>()))
                            .ReturnsAsync(false);


            // Act
            var result = await _employeeService.CreateEmployeeAsync(employeeDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
            Assert.Equal("Smith", result.Surname);
            _emailServiceMock.Verify(e => e.SendEmailAsync(
                employeeDto.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task CreateEmployeeAsyncDuplicateEmailThrowsArgumentException()
        {
            // Arrange
            var employeeDto = new CreateEmployeeRequestDto
            {
                Name = "Jane",
                Surname = "Doe",
                Email = "jane.doe@singular.co.za",
                ContactNumber = "0123456789",
                IdNumber = "0305054589589",
                PhysicalAddress = "123 Main St",
                TaxNumber = "1234567890",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Branch = "Johannesburg",
                City = "Johannesburg",
                ZipCode = "2000",
                Title = "Ms",
                Gender = "Female",
                PositionId = 1,
                MonthlySalary = 30000,
                EmploymentStatus = "Permanent",
                CareerManager = "Manager Name",
                EmpPicture = "profile.jpg"
            };

            _employeeRepoMock.Setup(r => r.EmailExistsAsync(employeeDto.Email))
                             .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _employeeService.CreateEmployeeAsync(employeeDto));
        }


        [Fact]
        public async Task CreateEmployeeAsyncInvalidTitleGenderThrowsArgumentException()
        {
            var employeeDto = new CreateEmployeeRequestDto
            {
                Name = "Alex",
                Surname = "King",
                Title = "Mr",
                Gender = "Female", // Invalid combination
                Email = "alex.king@singular.co.za",
                ContactNumber = "0123456789",
                PhysicalAddress = "123 Main St",
                IdNumber = "0305054589589",
                TaxNumber = "1234567890",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Branch = "Johannesburg",
                City = "Johannesburg",
                ZipCode = "2000",
                PositionId = 1,
                MonthlySalary = 20000,
                EmploymentStatus = "Permanent",
                CareerManager = "Manager Name",
                EmpPicture = "profile.jpg"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _employeeService.CreateEmployeeAsync(employeeDto));
        }

        [Fact]
        public async Task UpdateEmployeeAsyncValidInputReturnsUpdatedEmployee()
        {
            // Arrange
            var employeeId = "EMP001";

            var updateDto = new UpdateEmployeeRequestDto
            {
                Title = "Mr",
                Name = "UpdatedName",
                Surname = "UpdatedSurname",
                IdNumber = "0305054589589",
                PassportNumber = "",
                ContactNumber = "0987654321",
                Email = "updated@singular.co.za",
                PhysicalAddress = "456 New Street",
                City = "Cape Town",
                ZipCode = "8000",
                HasDisability = false,
                DisabilityDescription = null,
                Branch = "Cape Town",
                MonthlySalary = 35000,
                PositionId = 2,
                EmploymentStatus = "Permanent",
                CareerManager = "New Manager",
                EmpPicture = "updated.jpg"
            };

            var existingEmployee = new Employee
            {
                EmployeeId = employeeId,
                Name = "OldName",
                Surname = "OldSurname",
                Email = "old@email.com",
                IdNumber = "0305054589589"
            };

            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                             .ReturnsAsync(existingEmployee);

            _employeeRepoMock.Setup(r => r.UpdateEmployeeAsync(employeeId, It.IsAny<UpdateEmployeeRequestDto>()))
                .ReturnsAsync(new Employee
                {
                    EmployeeId = employeeId,
                    Name = "UpdatedName",
                    Surname = "UpdatedSurname",
                    Email = updateDto.Email
                });


            // Act
            var result = await _employeeService.UpdateEmployeeAsync(employeeId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UpdatedName", result.Name);
            Assert.Equal("UpdatedSurname", result.Surname);

            _employeeRepoMock.Verify(
                r => r.UpdateEmployeeAsync(employeeId, It.IsAny<UpdateEmployeeRequestDto>()),
                Times.Once
            );
        }


        [Fact]
        public async Task UpdateEmployeeAsyncEmployeeNotFoundThrowsArgumentException()
        {
            var employeeId = "EMP999";

            var updateDto = new UpdateEmployeeRequestDto
            {
                Title = "Mr",
                Name = "Test",
                Surname = "User",
                IdNumber = "0305054589589",
                ContactNumber = "0123456789",
                Email = "test@singular.co.za",
                PhysicalAddress = "123 Street",
                City = "Johannesburg",
                ZipCode = "2000",
                Branch = "Johannesburg",
                MonthlySalary = 20000,
                PositionId = 1,
                EmploymentStatus = "Permanent",
                CareerManager = "Manager",
                EmpPicture = "profile.jpg"
            };

            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                             .ReturnsAsync((Employee?)null);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _employeeService.UpdateEmployeeAsync(employeeId, updateDto));
        }


        [Fact]
        public async Task DeleteEmployeeAsyncValidIdReturnsTrue()
        {
            var employeeId = "EMP001";

            var employee = new Employee
            {
                EmployeeId = employeeId,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                             .ReturnsAsync(employee);

            _employeeRepoMock.Setup(r => r.DeleteEmployeeAsync(employeeId))
                             .ReturnsAsync(true);

            var result = await _employeeService.DeleteEmployeeAsync(employeeId);

            Assert.True(result);
        }


        [Fact]
        public async Task DeleteEmployeeAsyncEmployeeNotFoundThrowsArgumentException()
        {
            var employeeId = "EMP404";

            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                             .ReturnsAsync((Employee?)null);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _employeeService.DeleteEmployeeAsync(employeeId));
        }







    }
}