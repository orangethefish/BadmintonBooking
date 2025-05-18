# Badminton Booking API Tests

This project contains unit tests for the Badminton Booking backend API.

## Project Structure

The test project is organized to mirror the structure of the main project:

- **AuthServiceTests**: Tests for the authentication and user management service
- **FacilityServiceTests**: Tests for the facility management service
- **CourtServiceTests**: Tests for the court management service
- **AuthControllerTests**: Tests for the auth controller endpoints
- **FacilityControllerTests**: Tests for the facility controller endpoints

## Running Tests

To run the tests, use the following command from the root directory:

```
dotnet test Backend.Tests
```

## Test Setup

The tests use:

1. **In-memory database**: Each test uses an isolated in-memory database to ensure tests don't interfere with each other
2. **Moq**: For mocking dependencies and isolating the component being tested
3. **xUnit**: As the testing framework

## Troubleshooting

If you encounter reference errors when running the tests, ensure:

1. The project reference is correctly set up:
   ```
   dotnet add reference ../Backend/BadmintonBooking.API.csproj
   ```

2. All required NuGet packages are installed:
   ```
   dotnet add package Moq
   dotnet add package Microsoft.EntityFrameworkCore.InMemory
   ```

3. The namespace references in test files match the actual namespaces in the main project

## Best Practices

- Each test method should be independent and able to run in isolation
- Use descriptive test method names that indicate:
  - What is being tested
  - Under what conditions
  - What the expected outcome is
- Each test should focus on a single behavior
- Mock external dependencies to isolate the component being tested

## Test Coverage

These tests cover:

- Service layer functions
- Controller actions
- Authentication flows
- Error handling
- Data validation 