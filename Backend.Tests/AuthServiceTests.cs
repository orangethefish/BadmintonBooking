using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BadmintonBooking.API.Data;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services;
using BadmintonBooking.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Backend.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly Mock<IRoleService> _mockRoleService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public AuthServiceTests()
        {
            _mockLogger = new Mock<ILoggerService>();
            _mockRoleService = new Mock<IRoleService>();
            _mockTokenService = new Mock<ITokenService>();
            
            // Setup in-memory database for testing
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"AuthTestDb_{Guid.NewGuid()}")
                .Options;
        }

        private ApplicationDbContext CreateContext()
        {
            return new ApplicationDbContext(_options);
        }

        [Fact]
        public async Task RegisterAsync_ValidUser_ReturnsSuccessResult()
        {
            // Arrange
            var registerModel = new RegisterModel
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                AccountType = "User"
            };

            var token = "test.jwt.token";
            var roles = new List<string> { "User" };

            _mockRoleService.Setup(m => m.AddUserToRoleAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            
            _mockRoleService.Setup(m => m.GetUserRolesAsync(It.IsAny<Guid>()))
                .ReturnsAsync(roles);
            
            _mockTokenService.Setup(m => m.GenerateJwtToken(It.IsAny<User>(), It.IsAny<List<string>>()))
                .Returns(token);

            using (var context = CreateContext())
            {
                var authService = new AuthService(context, _mockTokenService.Object, _mockLogger.Object, _mockRoleService.Object);

                // Act
                var result = await authService.RegisterAsync(registerModel);

                // Assert
                Assert.True(result.Success);
                Assert.Equal(token, result.Token);
                Assert.Equal(registerModel.Username, result.Username);
                Assert.Equal(roles, result.Roles);
            }

            // Verify user was added to database
            using (var context = CreateContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == registerModel.Email);
                Assert.NotNull(user);
                Assert.Equal(registerModel.Username, user.Username);
            }
        }

        [Fact]
        public async Task RegisterAsync_DuplicateUsername_ReturnsFailure()
        {
            // Arrange
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "existing@example.com",
                PasswordHash = "hashedPassword",
                CreatedAt = DateTime.UtcNow
            };

            var registerModel = new RegisterModel
            {
                Username = "testuser", // Same username as existing user
                Email = "new@example.com",
                Password = "Password123!",
                AccountType = "User"
            };

            using (var context = CreateContext())
            {
                context.Users.Add(existingUser);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var authService = new AuthService(context, _mockTokenService.Object, _mockLogger.Object, _mockRoleService.Object);

                // Act
                var result = await authService.RegisterAsync(registerModel);

                // Assert
                Assert.False(result.Success);
                Assert.Contains("Username already exists", result.Error);
            }
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccessResult()
        {
            // Arrange
            string password = "Password123!";
            string hashedPassword;
            
            // Hash the password the same way as in AuthService
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                hashedPassword = Convert.ToBase64String(hashedBytes);
            }
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            var token = "test.jwt.token";
            var roles = new List<string> { "User" };

            _mockRoleService.Setup(m => m.GetUserRolesAsync(It.IsAny<Guid>()))
                .ReturnsAsync(roles);
            
            _mockTokenService.Setup(m => m.GenerateJwtToken(It.IsAny<User>(), It.IsAny<List<string>>()))
                .Returns(token);

            using (var context = CreateContext())
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var authService = new AuthService(context, _mockTokenService.Object, _mockLogger.Object, _mockRoleService.Object);

                // Act
                var result = await authService.LoginAsync(user.Email, password);

                // Assert
                Assert.True(result.Success);
                Assert.Equal(token, result.Token);
                Assert.Equal(user.Username, result.Username);
                Assert.Equal(roles, result.Roles);
            }
        }

        [Fact]
        public async Task LoginAsync_InvalidEmail_ReturnsFailure()
        {
            // Arrange
            using (var context = CreateContext())
            {
                var authService = new AuthService(context, _mockTokenService.Object, _mockLogger.Object, _mockRoleService.Object);

                // Act
                var result = await authService.LoginAsync("nonexistent@example.com", "anypassword");

                // Assert
                Assert.False(result.Success);
                Assert.Contains("User not found", result.Error);
            }
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsFailure()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedPassword", // This won't match any plain text password we provide
                CreatedAt = DateTime.UtcNow
            };

            using (var context = CreateContext())
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var authService = new AuthService(context, _mockTokenService.Object, _mockLogger.Object, _mockRoleService.Object);

                // Act
                var result = await authService.LoginAsync(user.Email, "wrongpassword");

                // Assert
                Assert.False(result.Success);
                Assert.Contains("Invalid password", result.Error);
            }
        }

        [Fact]
        public async Task LogoutAsync_ValidUser_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            using (var context = CreateContext())
            {
                var authService = new AuthService(context, _mockTokenService.Object, _mockLogger.Object, _mockRoleService.Object);

                // Act
                bool result = await authService.LogoutAsync(userId);

                // Assert
                Assert.True(result);
                _mockLogger.Verify(logger => logger.Info(It.IsAny<string>()), Times.Once);
            }
        }
    }
} 