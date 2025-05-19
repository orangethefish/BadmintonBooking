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
    public class CourtServiceTests
    {
        private readonly Mock<IFacilityService> _mockFacilityService;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly Guid _ownerId = Guid.NewGuid();
        private readonly int _facilityId = 1;

        public CourtServiceTests()
        {
            _mockFacilityService = new Mock<IFacilityService>();
            _mockLogger = new Mock<ILoggerService>();
            
            // Setup in-memory database for testing
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"CourtTestDb_{Guid.NewGuid()}")
                .Options;
        }

        private ApplicationDbContext CreateContext()
        {
            // Initialize with a facility
            var context = new ApplicationDbContext(_options);
            
            if (!context.Facilities.AnyAsync().Result)
            {
                context.Facilities.Add(new Facility
                {
                    Id = _facilityId,
                    Name = "Test Facility",
                    OwnerId = _ownerId,
                    Address = "123 Test Street",
                    PhoneNumber = "1234567890",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Description = "Test Facility"
                });
                context.SaveChanges();
            }
            
            return context;
        }

        [Fact]
        public async Task CreateCourtsAsync_ValidData_ReturnsCourts()
        {
            // Arrange
            string baseName = "Court";
            int numberOfCourts = 2;
            var pricingConfigurations = new List<PricingConfigurationRequest>
            {
                new PricingConfigurationRequest 
                { 
                    DayOfWeek = DayOfWeek.Monday,
                    StartTime = "08:00:00",
                    EndTime = "22:00:00",
                    Price = 20.0m,
                    HourlyRate = 20.0m
                }
            };

            using (var context = CreateContext())
            {
                var courtService = new CourtService(context, _mockFacilityService.Object);

                // Act
                var result = await courtService.CreateCourtsAsync(_facilityId, baseName, numberOfCourts, pricingConfigurations);

                // Assert
                Assert.NotNull(result);
                var courts = result.ToList();
                Assert.Equal(numberOfCourts, courts.Count);
                Assert.All(courts, c => Assert.Equal(_facilityId, c.FacilityId));
            }

            // Verify courts were added to database
            using (var context = CreateContext())
            {
                var courts = await context.Courts.Where(c => c.FacilityId == _facilityId).ToListAsync();
                Assert.Equal(numberOfCourts, courts.Count);
            }
        }

        [Fact]
        public async Task GetCourtAsync_ExistingId_ReturnsCourt()
        {
            // Arrange
            var court = new Court
            {
                Id = 1,
                Name = "Court 1",
                FacilityId = _facilityId,
                OwnerId = _ownerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            using (var context = CreateContext())
            {
                context.Courts.Add(court);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var courtService = new CourtService(context, _mockFacilityService.Object);

                // Act
                var result = await courtService.GetCourtAsync(court.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(court.Id, result.Id);
                Assert.Equal(court.Name, result.Name);
            }
        }

        [Fact]
        public async Task GetCourtAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var nonExistingId = 999;

            using (var context = CreateContext())
            {
                var courtService = new CourtService(context, _mockFacilityService.Object);

                // Act
                var result = await courtService.GetCourtAsync(nonExistingId);

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetCourtsAsync_ReturnsFacilityCourts()
        {
            // Arrange
            var facility1Id = _facilityId;
            var facility2Id = 2;

            // Add facility 2
            using (var context = CreateContext())
            {
                context.Facilities.Add(new Facility
                {
                    Id = facility2Id,
                    Name = "Test Facility 2",
                    OwnerId = _ownerId,
                    Address = "456 Test Street",
                    PhoneNumber = "0987654321",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Description = "Test Facility"
                });
                await context.SaveChangesAsync();
            }

            var facility1Courts = new List<Court>
            {
                new Court
                {
                    Id = 1,
                    Name = "Facility 1 Court 1",
                    FacilityId = facility1Id,
                    OwnerId = _ownerId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Court
                {
                    Id = 2,
                    Name = "Facility 1 Court 2",
                    FacilityId = facility1Id,
                    OwnerId = _ownerId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var facility2Courts = new List<Court>
            {
                new Court
                {
                    Id = 3,
                    Name = "Facility 2 Court",
                    FacilityId = facility2Id,
                    OwnerId = _ownerId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            using (var context = CreateContext())
            {
                context.Courts.AddRange(facility1Courts);
                context.Courts.AddRange(facility2Courts);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var courtService = new CourtService(context, _mockFacilityService.Object);

                // Act
                var result = await courtService.GetCourtsAsync(facility1Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                Assert.All(result, c => Assert.Equal(facility1Id, c.FacilityId));
            }
        }

        [Fact]
        public async Task UpdateCourtAsync_ValidData_UpdatesCourt()
        {
            // Arrange
            var court = new Court
            {
                Id = 1,
                Name = "Original Court",
                FacilityId = _facilityId,
                OwnerId = _ownerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var updateCourtRequest = new UpdateCourtRequest
            {
                Name = "Updated Court"
            };

            using (var context = CreateContext())
            {
                context.Courts.Add(court);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var courtService = new CourtService(context, _mockFacilityService.Object);

                // Act
                var result = await courtService.UpdateCourtAsync(court.Id, updateCourtRequest);

                // Assert
                Assert.True(result);
            }

            // Verify court was updated in database
            using (var context = CreateContext())
            {
                var updatedCourt = await context.Courts.FindAsync(court.Id);
                Assert.NotNull(updatedCourt);
                Assert.Equal(updateCourtRequest.Name, updatedCourt.Name);
            }
        }

        [Fact]
        public async Task DeleteCourtAsync_ExistingId_DeletesCourt()
        {
            // Arrange
            var court = new Court
            {
                Id = 1,
                Name = "Test Court",
                FacilityId = _facilityId,
                OwnerId = _ownerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            using (var context = CreateContext())
            {
                context.Courts.Add(court);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var courtService = new CourtService(context, _mockFacilityService.Object);

                // Act
                var result = await courtService.DeleteCourtAsync(court.Id);

                // Assert
                Assert.True(result);
            }

            // Verify court was deleted from database
            using (var context = CreateContext())
            {
                var deletedCourt = await context.Courts.FindAsync(court.Id);
                Assert.Null(deletedCourt);
            }
        }

        [Fact]
        public async Task CheckCourtAvailabilityAsync_NoOverlap_ReturnsTrue()
        {
            // Arrange
            var court = new Court
            {
                Id = 1,
                Name = "Test Court",
                FacilityId = _facilityId,
                OwnerId = _ownerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);

            using (var context = CreateContext())
            {
                context.Courts.Add(court);
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var courtService = new CourtService(context, _mockFacilityService.Object);

                // Act
                var result = await courtService.CheckCourtAvailabilityAsync(court.Id, startTime, endTime);

                // Assert
                Assert.True(result);
            }
        }
    }
} 