using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BadmintonBooking.API.Controllers;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services;
using BadmintonBooking.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests
{
    public class FacilityControllerTests
    {
        private readonly Mock<IFacilityService> _mockFacilityService;
        private readonly FacilityController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public FacilityControllerTests()
        {
            _mockFacilityService = new Mock<IFacilityService>();
            _controller = new FacilityController(_mockFacilityService.Object);
            
            // Setup user claims for authorization
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetFacility_ExistingId_ReturnsOkWithFacility()
        {
            // Arrange
            int facilityId = 1;
            var facility = new Facility
            {
                Id = facilityId,
                Name = "Test Facility",
                OwnerId = _userId,
                CreatedAt = DateTime.UtcNow
            };

            _mockFacilityService.Setup(s => s.GetFacilityAsync(facilityId))
                .ReturnsAsync(facility);

            // Act
            var result = await _controller.GetFacility(facilityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FacilityResponse>(okResult.Value);
            Assert.Equal(facilityId, returnValue.Id);
            Assert.Equal(facility.Name, returnValue.Name);
        }

        [Fact]
        public async Task GetFacility_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            int facilityId = 999;

            _mockFacilityService.Setup(s => s.GetFacilityAsync(facilityId))
                .ReturnsAsync((Facility)null);

            // Act
            var result = await _controller.GetFacility(facilityId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateFacility_ValidData_ReturnsOkWithFacility()
        {
            // Arrange
            var createFacilityRequest = new CreateFacilityRequest
            {
                Name = "New Facility",
                Address = "123 Test Street",
                PhoneNumber = "555-123-4567",
                Description = "A test facility"
            };

            var newFacility = new Facility
            {
                Id = 1,
                Name = createFacilityRequest.Name,
                Address = createFacilityRequest.Address,
                PhoneNumber = createFacilityRequest.PhoneNumber,
                Description = createFacilityRequest.Description,
                OwnerId = _userId,
                CreatedAt = DateTime.UtcNow
            };

            _mockFacilityService.Setup(s => s.CreateFacilityAsync(createFacilityRequest, _userId))
                .ReturnsAsync(newFacility);

            // Act
            var result = await _controller.CreateFacility(createFacilityRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FacilityResponse>(okResult.Value);
            Assert.Equal(newFacility.Id, returnValue.Id);
            Assert.Equal(newFacility.Name, returnValue.Name);
        }

        [Fact]
        public async Task ResolveUrl_ValidUrl_ReturnsOkWithResult()
        {
            // Arrange
            var request = new ResolveUrlRequest
            {
                Url = "https://maps.google.com/test-facility"
            };

            var response = new ResolveUrlResponse
            {
                Name = "Test Facility",
                FormattedAddress = "123 Test Street, Test City",
                PlaceId = "ChIJ12345abcde"
            };

            _mockFacilityService.Setup(s => s.ResolveUrlAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ResolveUrl(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ResolveUrlResponse>(okResult.Value);
            Assert.Equal(response.Name, returnValue.Name);
            Assert.Equal(response.FormattedAddress, returnValue.FormattedAddress);
        }

        [Fact]
        public async Task ResolveUrl_EmptyUrl_ReturnsBadRequest()
        {
            // Arrange
            var request = new ResolveUrlRequest
            {
                Url = ""
            };

            // Act
            var result = await _controller.ResolveUrl(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}