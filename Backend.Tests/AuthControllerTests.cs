using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BadmintonBooking.API.Controllers;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var authResult = new AuthResult
            {
                Success = true,
                Token = "test.jwt.token",
                Username = "testuser",
                Roles = new List<string> { "User" }
            };

            _mockAuthService.Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResult>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(authResult.Token, returnValue.Token);
            Assert.Equal(authResult.Username, returnValue.Username);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            var authResult = new AuthResult
            {
                Success = false,
                Error = "Invalid password"
            };

            _mockAuthService.Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResult>(badRequestResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal(authResult.Error, returnValue.Error);
        }

        [Fact]
        public async Task Login_ServiceThrowsException_ReturnsServerError()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            _mockAuthService.Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task Register_ValidData_ReturnsOkWithToken()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                AccountType = "User"
            };

            var authResult = new AuthResult
            {
                Success = true,
                Token = "test.jwt.token",
                Username = "testuser",
                Roles = new List<string> { "User" }
            };

            _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterModel>()))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResult>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(authResult.Token, returnValue.Token);
            Assert.Equal(authResult.Username, returnValue.Username);
        }

        [Fact]
        public async Task Register_DuplicateUsername_ReturnsBadRequest()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "existinguser",
                Email = "test@example.com",
                Password = "Password123!",
                AccountType = "User"
            };

            var authResult = new AuthResult
            {
                Success = false,
                Error = "Username already exists"
            };

            _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterModel>()))
                .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResult>(badRequestResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal(authResult.Error, returnValue.Error);
        }

        [Fact]
        public async Task Logout_ValidUser_ReturnsOk()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _mockAuthService.Setup(s => s.LogoutAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // We only need to validate the status code, no need to check the value type
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Logout_NoUserId_ReturnsBadRequest()
        {
            // Arrange
            // Set up controller with no user claims
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = await _controller.Logout();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task Logout_ServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _mockAuthService.Setup(s => s.LogoutAsync(userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Logout();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
    }
} 