using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BadmintonBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FacilityController : ControllerBase
    {
        private readonly IFacilityService _facilityService;

        public FacilityController(IFacilityService facilityService)
        {
            _facilityService = facilityService;
        }

        [HttpPost]
        public async Task<ActionResult<FacilityResponse>> CreateFacility(CreateFacilityRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
                var facility = await _facilityService.CreateFacilityAsync(request, userId);
                
                return Ok(new FacilityResponse
                {
                    Id = facility.Id,
                    Name = facility.Name,
                    Address = facility.Address,
                    PhoneNumber = facility.PhoneNumber,
                    Description = facility.Description,
                    IsActive = facility.IsActive,
                    OwnerId = facility.OwnerId,
                    CreatedAt = facility.CreatedAt,
                    UpdatedAt = facility.UpdatedAt,
                    MapsUrl = facility.MapsUrl,
                    CourtLatitude = facility.CourtLatitude,
                    CourtLongitude = facility.CourtLongitude,
                    PlaceId = facility.PlaceId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FacilityResponse>> GetFacility(int id)
        {
            try
            {
                var facility = await _facilityService.GetFacilityAsync(id);

                if (facility == null)
                {
                    return NotFound();
                }

                return Ok(new FacilityResponse
                {
                    Id = facility.Id,
                    Name = facility.Name,
                    Address = facility.Address,
                    PhoneNumber = facility.PhoneNumber,
                    Description = facility.Description,
                    IsActive = facility.IsActive,
                    OwnerId = facility.OwnerId,
                    CreatedAt = facility.CreatedAt,
                    UpdatedAt = facility.UpdatedAt,
                    MapsUrl = facility.MapsUrl,
                    CourtLatitude = facility.CourtLatitude,
                    CourtLongitude = facility.CourtLongitude,
                    PlaceId = facility.PlaceId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("resolve-url")]
        public async Task<ActionResult<ResolveUrlResponse>> ResolveUrl([FromBody] ResolveUrlRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Url))
                {
                    return BadRequest(new { error = "URL is required" });
                }

                var response = await _facilityService.ResolveUrlAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
} 