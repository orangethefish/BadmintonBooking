using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace BadmintonBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourtController : ControllerBase
    {
        private readonly ICourtService _courtService;
        private readonly ILogger<CourtController> _logger;

        public CourtController(ICourtService courtService, ILogger<CourtController> logger)
        {
            _courtService = courtService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<CourtResponse>> CreateCourt(CreateCourtRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
                var court = await _courtService.CreateCourtAsync(request, userId);
                
                return Ok(new CourtResponse
                {
                    Id = court.Id,
                    Name = court.Name,
                    FacilityId = court.FacilityId,
                    OwnerId = court.OwnerId,
                    IsActive = court.IsActive,
                    CreatedAt = court.CreatedAt,
                    UpdatedAt = court.UpdatedAt,
                    PricingConfigurations = court.PricingConfigurations?.Select(pc => new PricingConfigurationResponse
                    {
                        Id = pc.Id,
                        DayOfWeek = pc.DayOfWeek,
                        StartTime = pc.StartTime,
                        EndTime = pc.EndTime,
                        Price = pc.Price,
                        HourlyRate = pc.HourlyRate,
                        IsActive = pc.IsActive
                    }).ToList()
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating court");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("batch")]
        public async Task<ActionResult<List<CourtResponse>>> CreateCourtsBatch(BatchCreateCourtRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
                var courts = await _courtService.CreateCourtsBatchAsync(request, userId);
                
                return Ok(courts.Select(court => new CourtResponse
                {
                    Id = court.Id,
                    Name = court.Name,
                    FacilityId = court.FacilityId,
                    OwnerId = court.OwnerId,
                    IsActive = court.IsActive,
                    CreatedAt = court.CreatedAt,
                    UpdatedAt = court.UpdatedAt,
                    PricingConfigurations = court.PricingConfigurations?.Select(pc => new PricingConfigurationResponse
                    {
                        Id = pc.Id,
                        DayOfWeek = pc.DayOfWeek,
                        StartTime = pc.StartTime,
                        EndTime = pc.EndTime,
                        Price = pc.Price,
                        HourlyRate = pc.HourlyRate,
                        IsActive = pc.IsActive
                    }).ToList()
                }).ToList());
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating courts batch");
                return BadRequest(new { error = ex.Message });
            }
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
                        HourlyRate = pc.HourlyRate,
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
    }
} 