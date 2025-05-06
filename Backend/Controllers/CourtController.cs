using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services;
using System.Security.Claims;

namespace BadmintonBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourtController : ControllerBase
    {
        private readonly ICourtService _courtService;

        public CourtController(ICourtService courtService)
        {
            _courtService = courtService;
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
                    IsActive = court.IsActive,
                    CreatedAt = court.CreatedAt,
                    UpdatedAt = court.UpdatedAt
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
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
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
} 