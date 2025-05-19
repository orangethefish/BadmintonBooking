using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BadmintonBooking.API.Data;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services;
using BadmintonBooking.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Backend.Tests
{
    public class FacilityServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly Guid _ownerId = Guid.NewGuid();

        public FacilityServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            
            // Setup in-memory database for testing
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"FacilityTestDb_{Guid.NewGuid()}")
                .Options;
        }

        private ApplicationDbContext CreateContext()
        {
            return new ApplicationDbContext(_options);
        }

        [Fact]
        public async Task CreateFacilityAsync_ValidData_ReturnsFacility()
        {
            // Arrange
            var createFacilityRequest = new CreateFacilityRequest
            {
                Name = "Test Badminton Center",
                Address = "123 Test Street",
                PhoneNumber = "555-123-4567",
                Description = "A test facility for badminton"
            };

            using (var context = CreateContext())
            {
                var facilityService = new FacilityService(context, _mockConfiguration.Object, _mockHttpClientFactory.Object);

                // Act
                var result = await facilityService.CreateFacilityAsync(createFacilityRequest, _ownerId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(createFacilityRequest.Name, result.Name);
                Assert.Equal(_ownerId, result.OwnerId);
            }

            // Verify facility was added to database
            using (var context = CreateContext())
            {
                var facility = await context.Facilities.FirstOrDefaultAsync();
                Assert.NotNull(facility);
                Assert.Equal(createFacilityRequest.Name, facility.Name);
                Assert.Equal(_ownerId, facility.OwnerId);
            }
        }

        [Fact]
        public async Task GetFacilityAsync_ExistingId_ReturnsFacility()
        {
            // Arrange
            var facility = new Facility
            {
                Id = 1,
                Name = "Test Badminton Center",
                Address = "123 Test Street",
                PhoneNumber = "555-123-4567",
                Description = "A test facility for badminton",
                OwnerId = _ownerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            using (var context = CreateContext())
            {
                context.Facilities.Add(facility);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var facilityService = new FacilityService(context, _mockConfiguration.Object, _mockHttpClientFactory.Object);

                // Act
                var result = await facilityService.GetFacilityAsync(facility.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(facility.Id, result.Id);
                Assert.Equal(facility.Name, result.Name);
            }
        }

        [Fact]
        public async Task GetFacilityAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var nonExistingId = 999;

            using (var context = CreateContext())
            {
                var facilityService = new FacilityService(context, _mockConfiguration.Object, _mockHttpClientFactory.Object);

                // Act
                var result = await facilityService.GetFacilityAsync(nonExistingId);

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetUserFacilitiesAsync_ReturnsOwnerFacilities()
        {
            // Arrange
            var owner1Id = _ownerId;
            var owner2Id = Guid.NewGuid();

            var owner1Facilities = new List<Facility>
            {
                new Facility
                {
                    Id = 1,
                    Name = "Owner 1 Facility 1",
                    Address = "123 Facility 1 Street",
                    PhoneNumber = "555-111-1111",
                    OwnerId = owner1Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Description = "Test Facility 1"
                },
                new Facility
                {
                    Id = 2,
                    Name = "Owner 1 Facility 2",
                    Address = "456 Facility 2 Street",
                    PhoneNumber = "555-222-2222",
                    OwnerId = owner1Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Description = "Test Facility 2"
                }
            };

            var owner2Facilities = new List<Facility>
            {
                new Facility
                {
                    Id = 3,
                    Name = "Owner 2 Facility",
                    Address = "789 Facility 3 Street",
                    PhoneNumber = "555-333-3333",
                    OwnerId = owner2Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Description = "Test Facility 3"
                }
            };

            using (var context = CreateContext())
            {
                context.Facilities.AddRange(owner1Facilities);
                context.Facilities.AddRange(owner2Facilities);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var facilityService = new FacilityService(context, _mockConfiguration.Object, _mockHttpClientFactory.Object);

                // Act
                var result = await facilityService.GetUserFacilitiesAsync(owner1Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                Assert.All(result, f => Assert.Equal(owner1Id, f.OwnerId));
            }
        }

        [Fact]
        public async Task UpdateFacilityAsync_ValidData_UpdatesFacility()
        {
            // Arrange
            var facility = new Facility
            {
                Id = 1,
                Name = "Original Name",
                Address = "Original Address",
                PhoneNumber = "555-123-4567",
                Description = "Original Description",
                OwnerId = _ownerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var updateFacilityRequest = new UpdateFacilityRequest
            {
                Name = "Updated Name",
                Address = "Updated Address",
                PhoneNumber = "555-987-6543",
                Description = "Updated Description"
            };

            using (var context = CreateContext())
            {
                context.Facilities.Add(facility);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var facilityService = new FacilityService(context, _mockConfiguration.Object, _mockHttpClientFactory.Object);

                // Act
                var result = await facilityService.UpdateFacilityAsync(facility.Id, updateFacilityRequest);

                // Assert
                Assert.True(result);
            }

            // Verify facility was updated in database
            using (var context = CreateContext())
            {
                var updatedFacility = await context.Facilities.FindAsync(facility.Id);
                Assert.NotNull(updatedFacility);
                Assert.Equal(updateFacilityRequest.Name, updatedFacility.Name);
                Assert.Equal(updateFacilityRequest.Address, updatedFacility.Address);
                Assert.Equal(updateFacilityRequest.PhoneNumber, updatedFacility.PhoneNumber);
            }
        }

        [Fact]
        public async Task DeleteFacilityAsync_ExistingId_DeletesFacility()
        {
            // Arrange
            var facility = new Facility
            {
                Id = 1,
                Name = "Test Facility",
                Address = "123 Test Street",
                PhoneNumber = "555-123-4567",
                OwnerId = _ownerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Description = "Test facility for deletion"
            };

            using (var context = CreateContext())
            {
                context.Facilities.Add(facility);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var facilityService = new FacilityService(context, _mockConfiguration.Object, _mockHttpClientFactory.Object);

                // Act
                var result = await facilityService.DeleteFacilityAsync(facility.Id);

                // Assert
                Assert.True(result);
            }

            // Verify facility was deleted from database
            using (var context = CreateContext())
            {
                var deletedFacility = await context.Facilities.FindAsync(facility.Id);
                Assert.Null(deletedFacility);
            }
        }

        [Fact]
        public async Task IsFacilityOwnerAsync_CorrectOwner_ReturnsTrue()
        {
            // Arrange
            var facility = new Facility
            {
                Id = 1,
                Name = "Test Facility",
                Address = "123 Test Street",
                PhoneNumber = "555-123-4567",
                OwnerId = _ownerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Description = "Test facility for ownership check"
            };

            using (var context = CreateContext())
            {
                context.Facilities.Add(facility);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var facilityService = new FacilityService(context, _mockConfiguration.Object, _mockHttpClientFactory.Object);

                // Act
                var result = await facilityService.IsFacilityOwnerAsync(facility.Id, _ownerId);

                // Assert
                Assert.True(result);
            }
        }
    }
} 