namespace HRConnect.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs.Employee;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using System.Globalization;
    using HRConnect.Api.Mappers;
    using System.Data.Common;
    /// <summary>
    /// Handles business logic related to Employee operations.
    /// This layer is the bridge between the Controller and Repository.
    /// Responsible for validation, duplicate checks, ID generation and email notifications. - O.Seilane
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IEmailService _emailService;
        public EmployeeService(IEmployeeRepository employeeRepo, IEmailService emailService)
        {
            _employeeRepo = employeeRepo;
            _emailService = emailService;
        }
        /// <summary>
        /// Retrieves all employees from the repository.
        /// </summary>
        /// <returns>A list of all employees.</returns>
        public Task<List<Employee>> GetAllEmployeesAsync()
        {
            return _employeeRepo.GetAllEmployeesAsync();
        }
        /// <summary>
        /// Retrieves a single employee by their Employee ID.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier.</param>
        /// <returns>The employee if found; otherwise null.</returns>
        public Task<Employee?> GetEmployeeByIdAsync(string EmployeeId)
        {
            return _employeeRepo.GetEmployeeByIdAsync(EmployeeId);
        }
        /// <summary>
        /// Creates a new employee after validating input, checking duplicates,
        /// generating a unique Employee ID, auto generate DOB and Gender if ID is provided and sending a welcome email.
        /// </summary>
        /// <param name="employeeRequestDto">Employee creation request data.</param>
        /// <returns>The newly created employee entity and the Welcome email sent.</returns>
        public async Task<Employee> CreateEmployeeAsync(CreateEmployeeRequestDto employeeRequestDto)
        {
            // Validate incoming employee data
            ValidateEmployee(employeeRequestDto);
            // Ensure no duplicates exist
            await CheckForDuplicates(employeeRequestDto);
            // If ID number exists, auto-extract DOB and Gender
            if (!string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber))
            {
                var employeeinfo = IdNumberValidator.ParseIdNumber(employeeRequestDto.IdNumber);

                IdNumberValidator.AgeValidator.EnsureAdult(employeeinfo.DateOfBirth);
                employeeRequestDto.Gender = employeeinfo.Gender;
                employeeRequestDto.DateOfBirth = employeeinfo.DateOfBirth;
            }
            // Ensure Title and Gender combination is valid
            ValidateTitleAndGender(employeeRequestDto);
            employeeRequestDto.EmployeeId = await GenerateUniqueEmpId(employeeRequestDto.Surname);
            var new_employee = employeeRequestDto.ToEmployeeFromCreateDTO();
            var createdEmployee = await _employeeRepo.CreateEmployeeAsync(new_employee);
            // Send welcome email notification
            await SendWelcomeEmail(createdEmployee);
            return createdEmployee;

        }
        /// <summary>
        /// Updates an existing employee after validation and duplicate checks.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier.</param>
        /// <param name="employeeDto">Updated employee data.</param>
        /// <returns>The updated employee or null if not found.</returns>
        public async Task<Employee?> UpdateEmployeeAsync(string EmployeeId, UpdateEmployeeRequestDto employeeDto)
        {
            ValidateUpdateEmployee(employeeDto);
            await CheckForDuplicatesonUpdate(EmployeeId, employeeDto);

            return await _employeeRepo.UpdateEmployeeAsync(EmployeeId, employeeDto);

        }
        /// <summary>
        /// Deletes an employee if they exist and were started within the current month.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier.</param>
        /// <returns>True if deletion successful.</returns>
        public async Task<bool> DeleteEmployeeAsync(string EmployeeId)
        {
            var existingEmployee = await _employeeRepo.GetEmployeeByIdAsync(EmployeeId);

            if (existingEmployee == null)
                throw new ArgumentException("Employee not found");

            var now = DateTime.UtcNow;
            // Business rule: Only allow deletion within the same start month - O.Seilane
            if (existingEmployee.StartDate.Year != now.Year || existingEmployee.StartDate.Month != now.Month)
            {
                throw new ArgumentException("Employee can only be deleted in the same month they started.");
            }
            return await _employeeRepo.DeleteEmployeeAsync(EmployeeId);
        }
        /// <summary>
        /// Generates a unique Employee ID based on surname prefix and existing IDs.
        /// Example: SMI001 for surname Smith.
        /// </summary>
        /// <param name="lastName">The employee last name</param>
        /// <returns>New unique employee ID</returns>
        private async Task<string> GenerateUniqueEmpId(string lastName)
        {   // Create 3-letter prefix, by exstracting the first 3 letters of the last name and converting to uppercase. 
            // If last name is less than 3 letters, pad with 'X'. - O.Seilane
            string prefix = lastName.Length >= 3
                ? lastName.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture)
                : lastName.ToUpper(CultureInfo.InvariantCulture).PadRight(3, 'X');
            int nextNum = 1;
            // Fetch existing IDs with same prefix to determine the next number to use - O.Seilane
            var existingIds = await _employeeRepo.GetAllEmployeeIdsWithPrefix(prefix);
            if (existingIds.Count > 0)
            {
                // Extract numeric portion and increment to get the next number
                var maxNum = existingIds
                        .Select(id => int.Parse(id.AsSpan(3), CultureInfo.InvariantCulture))
                        .Max();

                nextNum = maxNum + 1;
            }

            return $"{prefix}{nextNum:D3}";
        }
        /// <summary>
        /// Sends a welcome email to a newly created employee's email address.
        /// </summary>
        /// <param name="employee">The employee object</param>
        /// <returns>True; If email was sent successfully</returns>
        private async Task SendWelcomeEmail(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Email))
                return;

            var subject = "Welcome to HRConnect";

            var body = $@"
                Hello {employee.Name} {employee.Surname},
                
                Welcome to HRConnect!
                
                Your employee ID: {employee.EmployeeId}
                Position: {employee.PositionId}
                Branch: {employee.Branch}
                
                We are glad to have you onboard. :-)";

            await _emailService.SendEmailAsync(employee.Email, subject, body);
        }
        /// <summary>
        /// /// <summary>
        /// Validates employee creation rules and throws exceptions if invalid.
        /// </summary>
        /// </summary>
        /// <param name="employeeRequestDto">The employee request DTO</param>
        /// <returns>Validation messages if input is invalid</returns>
        // Validates the employee data based on the specified rules and throws an ArgumentException if any validation fails. - O.Seilane
        private static void ValidateEmployee(CreateEmployeeRequestDto employeeRequestDto)
        {
            // Validation rules, allowed lists, and business checks live here.
            // Throws ArgumentException when any rule fails. - O.Seilane
            var allowedGenders = new[] { "Male", "Female" };
            var allowedTitles = new[] { "Mr", "Mrs", "Ms", "Dr", "Prof" };
            var allowedBranches = new[] { "Johannesburg", "Cape Town", "UK" };
            var allowedCities = new[] { "Johannesburg", "Cape Town", "Pretoria", "Durban", "Bloemfontein" };
            var allowedStatus = new[] { "Permanent", "Fixed-Term", "Contract" };
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var now = DateTime.UtcNow;
            bool isIdNumberProvided = !string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber);
            bool isPassportProvided = !string.IsNullOrWhiteSpace(employeeRequestDto.PassportNumber);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (string.IsNullOrWhiteSpace(employeeRequestDto.Name))
                throw new ArgumentException("Employee name is required");

            if (!isIdNumberProvided && !isPassportProvided)
                throw new ArgumentException("Either National ID or Passport is required");

            if (employeeRequestDto.StartDate.Month != now.Month || employeeRequestDto.StartDate.Year != now.Year)
            {
                throw new ArgumentException(
                    "Start date must be within the current month."
                );
            }

            if (isIdNumberProvided && isPassportProvided)
                throw new ArgumentException("You cannot enter both ID Number and Passport Number");

            if (isPassportProvided && !isIdNumberProvided)
            {
                if (employeeRequestDto.DateOfBirth == default)
                    throw new ArgumentException("Date of Birth is required if ID Number is not provided");

                IdNumberValidator.AgeValidator.EnsureAdult(employeeRequestDto.DateOfBirth);

                if (!allowedGenders.Contains(employeeRequestDto.Gender, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException("Gender must be either Male or Female");

                if (string.IsNullOrWhiteSpace(employeeRequestDto.Gender))
                    throw new ArgumentException("Employee Gender is required");
            }

            if (employeeRequestDto.Name.Length > 50)
                throw new ArgumentException("Employee name must not exceed 50 characters");

            if (!allowedTitles.Contains(employeeRequestDto.Title, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Employee Title must be either 'Mr', 'Mrs', 'Ms', 'Dr', 'Prof' ");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.Surname))
                throw new ArgumentException("Employee surname is required");

            if (employeeRequestDto.Surname.Length > 100)
                throw new ArgumentException("Employee name must not exceed 100 characters");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber) && employeeRequestDto.IdNumber.Length != 13)
                throw new ArgumentException("ID Number must be 13 digits long");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.TaxNumber) && employeeRequestDto.TaxNumber.Length != 10)
                throw new ArgumentException("Tax Number must be 10 digits long");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.ZipCode) && employeeRequestDto.ZipCode.Length != 4)
                throw new ArgumentException("Zip Code must be 4 digits long");

            if (employeeRequestDto.HasDisability && string.IsNullOrWhiteSpace(employeeRequestDto.DisabilityDescription))
                throw new ArgumentException("Disability description is required if HasDisability is true.");

            if (!employeeRequestDto.HasDisability && !string.IsNullOrWhiteSpace(employeeRequestDto.DisabilityDescription))
                throw new ArgumentException("Disability description must be empty if HasDisability is false.");
            if (!allowedCities.Contains(employeeRequestDto.City, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Employee City must be either 'Johannesburg', 'Cape Town', 'Pretoria', 'Durban', 'Bloemfontein'");
            CityZipValidator.ValidateCityAndZip(
                employeeRequestDto.City,
                employeeRequestDto.ZipCode
            );

            if (string.IsNullOrWhiteSpace(employeeRequestDto.ContactNumber))
                throw new ArgumentException("Employee contact number is required");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.City))
                throw new ArgumentException("Employee City is required");

            if (employeeRequestDto.ContactNumber.Length != 10)
                throw new ArgumentException("Contact number must be 10 digits long");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.Email) || !employeeRequestDto.Email.EndsWith("@singular.co.za", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Email must end with '@singular.co.za'");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.PhysicalAddress))
                throw new ArgumentException("Employee physical address is required");

            if (employeeRequestDto.StartDate == default)
                throw new ArgumentException("Start date is required");

            if (!allowedBranches.Contains(employeeRequestDto.Branch, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Branch must either be 'Johannesburg', 'Cape Town' or 'UK'");

            if (employeeRequestDto.MonthlySalary <= 0)
                throw new ArgumentException("Monthly salary must be greater than 0");

            if (employeeRequestDto.MonthlySalary >= 100000)
                throw new ArgumentException("Monthly salary must not exceed 100 000");

            if (employeeRequestDto.PositionId <= 0)
                throw new ArgumentException("Position ID must be greater than 0");

            if (!allowedStatus.Contains(employeeRequestDto.EmploymentStatus, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Employment status must either be 'Permanent', 'Fixed-Term' or 'Contract'");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.CareerManager))
                throw new ArgumentException("Employee career manager is required");

            var extension = Path.GetExtension(employeeRequestDto.EmpPicture);

            if (string.IsNullOrWhiteSpace(extension) ||
                !allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Employee picture must be a valid image file (.png, .jpg, .jpeg, .gif, .bmp, .webp)");
            }


        }
        /// <summary>
        /// Validates employee update rules.
        /// </summary>
        /// <param name="employeeRequestDto">The employee update request DTO</param>
        /// <returns>Validation messages if update input is invalid</returns>
        private static void ValidateUpdateEmployee(UpdateEmployeeRequestDto employeeRequestDto)
        {
            // Similar to create validation but excludes certain creation-only checks. - O.Seilane
            var allowedTitles = new[] { "Mr", "Mrs", "Ms", "Dr", "Prof" };
            var allowedBranches = new[] { "Johannesburg", "Cape Town", "UK" };
            var allowedCities = new[] { "Johannesburg", "Cape Town", "Pretoria", "Durban", "Bloemfontein" };
            var allowedStatus = new[] { "Permanent", "Fixed-Term", "Contract" };
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            bool isIdNumberProvided = !string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber);
            bool isPassportProvided = !string.IsNullOrWhiteSpace(employeeRequestDto.PassportNumber);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (string.IsNullOrWhiteSpace(employeeRequestDto.Name))
                throw new ArgumentException("Employee name is required");

            if (!isIdNumberProvided && !isPassportProvided)
                throw new ArgumentException("Either National ID or Passport is required");

            if (isIdNumberProvided && isPassportProvided)
                throw new ArgumentException("You cannot enter both ID Number and Passport Number");

            if (employeeRequestDto.Name.Length > 50)
                throw new ArgumentException("Employee name must not exceed 50 characters");

            if (!allowedTitles.Contains(employeeRequestDto.Title, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Employee Title must be either 'Mr', 'Mrs', 'Ms', 'Dr', 'Prof' ");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.Surname))
                throw new ArgumentException("Employee surname is required");

            if (employeeRequestDto.Surname.Length > 100)
                throw new ArgumentException("Employee name must not exceed 100 characters");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber) && employeeRequestDto.IdNumber.Length != 13)
                throw new ArgumentException("ID Number must be 13 digits long");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.ZipCode) && employeeRequestDto.ZipCode.Length != 4)
                throw new ArgumentException("Zip Code must be 4 digits long");

            if (employeeRequestDto.HasDisability && string.IsNullOrWhiteSpace(employeeRequestDto.DisabilityDescription))
                throw new ArgumentException("Disability description is required if HasDisability is true.");

            if (!employeeRequestDto.HasDisability && !string.IsNullOrWhiteSpace(employeeRequestDto.DisabilityDescription))
                throw new ArgumentException("Disability description must be empty if HasDisability is false.");
            if (!allowedCities.Contains(employeeRequestDto.City, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Employee City must be either 'Johannesburg', 'Cape Town', 'Pretoria', 'Durban', 'Bloemfontein'");
            CityZipValidator.ValidateCityAndZip(
                employeeRequestDto.City,
                employeeRequestDto.ZipCode
            );
            if (string.IsNullOrWhiteSpace(employeeRequestDto.ContactNumber))
                throw new ArgumentException("Employee contact number is required");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.City))
                throw new ArgumentException("Employee City is required");

            if (employeeRequestDto.ContactNumber.Length != 10)
                throw new ArgumentException("Contact number must be 10 digits long");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.Email) || !employeeRequestDto.Email.EndsWith("@singular.co.za", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Email must end with '@singular.co.za'");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.PhysicalAddress))
                throw new ArgumentException("Employee physical address is required");

            if (!allowedBranches.Contains(employeeRequestDto.Branch, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Branch must either be 'Johannesburg', 'Cape Town' or 'UK'");

            if (employeeRequestDto.MonthlySalary <= 0)
                throw new ArgumentException("Monthly salary must be greater than 0");

            if (employeeRequestDto.MonthlySalary >= 100000)
                throw new ArgumentException("Monthly salary must not exceed 100 000");

            if (employeeRequestDto.PositionId <= 0)
                throw new ArgumentException("Position ID must be greater than 0");

            if (!allowedStatus.Contains(employeeRequestDto.EmploymentStatus, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Employment status must either be 'Permanent', 'Fixed-Term' or 'Contract'");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.CareerManager))
                throw new ArgumentException("Employee career manager is required");
            //Exstracts the extenstion from a filename.
            var extension = Path.GetExtension(employeeRequestDto.EmpPicture);

            if (string.IsNullOrWhiteSpace(extension) ||
                !allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Employee picture must be a valid image file (.png, .jpg, .jpeg, .gif, .bmp, .webp)");
            }



        }
        /// <summary>
        /// Checks for duplicate records before creating a new employee.
        /// </summary>
        /// </summary>
        /// <param name="employeeRequestDto">The employee request DTO</param>
        /// <returns>Error message if duplicate is found</returns>
        private async Task CheckForDuplicates(CreateEmployeeRequestDto employeeRequestDto)
        {
            if (await _employeeRepo.EmailExistsAsync(employeeRequestDto.Email))
                throw new ArgumentException("An employee with the same email already exists");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.TaxNumber) && await _employeeRepo.TaxNumberExistsAsync(employeeRequestDto.TaxNumber))
                throw new ArgumentException("An employee with the same tax number already exists");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber) && await _employeeRepo.IdNumberExistsAsync(employeeRequestDto.IdNumber))
                throw new ArgumentException("An employee with the same ID number already exists");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.PassportNumber) && await _employeeRepo.PassportExistsAsync(employeeRequestDto.PassportNumber))
                throw new ArgumentException("An employee with the same passport number already exists");

            if (await _employeeRepo.ContactExistsAsync(employeeRequestDto.ContactNumber))
                throw new ArgumentException("An employee with the same contact number already exists");
        }
        /// <summary>
        /// Checks for duplicate records when updating an employee.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier</param>
        /// <param name="employeeRequestDto">The employee update request DTO</param>
        /// <returns>Error message if duplicate is found</returns>
        private async Task CheckForDuplicatesonUpdate(string EmployeeId, UpdateEmployeeRequestDto employeeRequestDto)
        {
            var existingEmployee = await _employeeRepo.GetEmployeeByIdAsync(EmployeeId);

            if (existingEmployee == null)
                throw new ArgumentException("Employee not found");

            if (await _employeeRepo.EmailExistsForOthersAsync(employeeRequestDto.Email, EmployeeId))
                throw new ArgumentException("Another employee with the same email already exists");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber) && await _employeeRepo.IdNumberExistsForOthersAsync(employeeRequestDto.IdNumber, EmployeeId))
                throw new ArgumentException("Another employee with the same ID number already exists");

            if (!string.IsNullOrWhiteSpace(employeeRequestDto.PassportNumber) && await _employeeRepo.PassportExistsForOthersAsync(employeeRequestDto.PassportNumber, EmployeeId))
                throw new ArgumentException("Another employee with the same passport number already exists");

            if (await _employeeRepo.ContactExistsForOthersAsync(employeeRequestDto.ContactNumber, EmployeeId))
                throw new ArgumentException("Another employee with the same contact number already exists");

        }
        /// <summary>
        /// Ensures title and gender combinations are logically valid.
        /// </summary>
        /// <param name="employeeRequestDto">The employee request DTO</param>
        /// <returns>Validation messages if title and gender are not logically valid</returns>
        private static void ValidateTitleAndGender(CreateEmployeeRequestDto employeeRequestDto)
        {
            if (employeeRequestDto.Title == "Mr")
            {
                if (employeeRequestDto.Gender != "Male")
                    throw new ArgumentException("Title 'Mr' must have gender 'Male'");
            }
            else if (employeeRequestDto.Title == "Mrs" || employeeRequestDto.Title == "Ms")
            {
                if (employeeRequestDto.Gender != "Female")
                    throw new ArgumentException("Title 'Mrs' or 'Ms' must have gender 'Female'");
            }
        }

    }
}