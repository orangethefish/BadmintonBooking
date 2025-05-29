using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services;
using BadmintonBooking.API.Services.Interfaces;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacilityResponse>>> GetUserFacilities()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
                var facilities = await _facilityService.GetUserFacilitiesAsync(userId);
                
                var response = facilities.Select(facility => new FacilityResponse
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

                return Ok(response);
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

        [HttpPut("{id}")]
        public async Task<ActionResult<FacilityResponse>> UpdateFacility(int id, UpdateFacilityRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
                // Check if user is facility owner
                bool isOwner = await _facilityService.IsFacilityOwnerAsync(id, userId);
                if (!isOwner)
                {
                    return Forbid();
                }

                var updated = await _facilityService.UpdateFacilityAsync(id, request);
                
                if (!updated)
                {
                    return NotFound();
                }

                // Get the updated facility to return
                var facility = await _facilityService.GetFacilityAsync(id);

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