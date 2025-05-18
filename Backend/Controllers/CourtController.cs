using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace BadmintonBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CourtController : ControllerBase
    {
        private readonly ICourtService _courtService;
        private readonly IFacilityService _facilityService;
        private readonly ILogger<CourtController> _logger;

        public CourtController(ICourtService courtService, IFacilityService facilityService, ILogger<CourtController> logger)
        {
            _courtService = courtService;
            _facilityService = facilityService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourtResponse>>> GetCourts([FromQuery] int facilityId)
        {
            try
            {
                var courts = await _courtService.GetCourtsAsync(facilityId);

                return Ok(courts.Select(c => new CourtResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    FacilityId = c.FacilityId,
                    OwnerId = c.OwnerId,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    PricingConfigurations = c.PricingConfigurations?.Select(pc => new PricingConfigurationResponse
                    {
                        Id = pc.Id,
                        DayOfWeek = pc.DayOfWeek,
                        StartTime = pc.StartTime,
                        EndTime = pc.EndTime,
                        Price = pc.Price,
                        IsActive = pc.IsActive
                    }).ToList()
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courts");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("facility/{facilityId}")]
        public async Task<ActionResult<object>> GetFacilityForCourtCreation(int facilityId)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                // Check if user is facility owner
                bool isOwner = await _facilityService.IsFacilityOwnerAsync(facilityId, userId);
                if (!isOwner)
                {
                    return Forbid();
                }

                // Get facility details
                var facility = await _facilityService.GetFacilityAsync(facilityId);
                if (facility == null)
                {
                    return NotFound(new { error = "Facility not found" });
                }

                return Ok(new
                {
                    facilityId = facility.Id,
                    name = facility.Name,
                    address = facility.Address
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility for court creation");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("batch")]
        public async Task<ActionResult<IEnumerable<CourtResponse>>> CreateCourts([FromBody] BatchCreateCourtRequest request)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                // Check if user is facility owner
                bool isOwner = await _facilityService.IsFacilityOwnerAsync(request.FacilityId, userId);
                if (!isOwner)
                {
                    return Forbid();
                }

                // Validate the request
                if (request.NumberOfCourts <= 0)
                {
                    return BadRequest(new { error = "Number of courts must be greater than 0" });
                }

                if (string.IsNullOrWhiteSpace(request.BaseName))
                {
                    return BadRequest(new { error = "Base name is required" });
                }

                if (request.PricingConfigurations == null || !request.PricingConfigurations.Any())
                {
                    return BadRequest(new { error = "At least one pricing configuration is required" });
                }

                // Process pricing configurations
                var pricingConfigs = new List<PricingConfigurationRequest>();
                foreach (var config in request.PricingConfigurations)
                {
                    pricingConfigs.Add(new PricingConfigurationRequest
                    {
                        DayOfWeek = config.DayOfWeek,
                        StartTime = config.StartTime,
                        EndTime = config.EndTime,
                        Price = config.Price,
                        HourlyRate = config.HourlyRate
                    });
                }

                // Create courts
                var courts = await _courtService.CreateCourtsAsync(
                    request.FacilityId,
                    request.BaseName,
                    request.NumberOfCourts,
                    pricingConfigs
                );

                // Map to response
                var response = courts.Select(c => new CourtResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    FacilityId = c.FacilityId,
                    OwnerId = c.OwnerId,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    PricingConfigurations = c.PricingConfigurations?.Select(pc => new PricingConfigurationResponse
                    {
                        Id = pc.Id,
                        DayOfWeek = pc.DayOfWeek,
                        StartTime = pc.StartTime,
                        EndTime = pc.EndTime,
                        Price = pc.Price,
                        IsActive = pc.IsActive
                    }).ToList()
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating courts");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
} 