using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BadmintonBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetUserBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            
            var bookingDtos = bookings.Select(b => new BookingResponseDto
            {
                Id = b.Id,
                CourtId = b.CourtId,
                CourtName = b.Court.Name,
                FacilityName = b.Court.Facility.Name,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                Notes = b.Notes,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            });

            return Ok(bookingDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBooking(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var booking = await _bookingService.GetBookingByIdAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            // Check if the booking belongs to the current user
            if (booking.UserId != userId)
            {
                return Forbid();
            }

            var bookingDto = new BookingResponseDto
            {
                Id = booking.Id,
                CourtId = booking.CourtId,
                CourtName = booking.Court.Name,
                FacilityName = booking.Court.Facility.Name,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                Notes = booking.Notes,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };

            return Ok(bookingDto);
        }

        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking(BookingRequestDto bookingDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Validate booking time
            if (bookingDto.StartTime >= bookingDto.EndTime)
            {
                return BadRequest("End time must be after start time");
            }

            if (bookingDto.StartTime < DateTime.UtcNow)
            {
                return BadRequest("Cannot book in the past");
            }

            // Check if the court is available
            bool isAvailable = await _bookingService.IsCourtAvailableAsync(
                bookingDto.CourtId, 
                bookingDto.StartTime, 
                bookingDto.EndTime
            );

            if (!isAvailable)
            {
                return BadRequest("The court is not available for the selected time period");
            }

            // Create booking
            var booking = new Booking
            {
                CourtId = bookingDto.CourtId,
                UserId = userId,
                StartTime = bookingDto.StartTime,
                EndTime = bookingDto.EndTime,
                Notes = bookingDto.Notes,
                Status = "Confirmed"
            };

            try
            {
                var createdBooking = await _bookingService.CreateBookingAsync(booking);

                var responseDto = new BookingResponseDto
                {
                    Id = createdBooking.Id,
                    CourtId = createdBooking.CourtId,
                    CourtName = createdBooking.Court.Name,
                    FacilityName = createdBooking.Court.Facility.Name,
                    StartTime = createdBooking.StartTime,
                    EndTime = createdBooking.EndTime,
                    TotalPrice = createdBooking.TotalPrice,
                    Status = createdBooking.Status,
                    Notes = createdBooking.Notes,
                    CreatedAt = createdBooking.CreatedAt,
                    UpdatedAt = createdBooking.UpdatedAt
                };

                return CreatedAtAction(nameof(GetBooking), new { id = createdBooking.Id }, responseDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, BookingRequestDto bookingDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingBooking = await _bookingService.GetBookingByIdAsync(id);

            if (existingBooking == null)
            {
                return NotFound();
            }

            // Check if the booking belongs to the current user
            if (existingBooking.UserId != userId)
            {
                return Forbid();
            }

            // Validate booking time
            if (bookingDto.StartTime >= bookingDto.EndTime)
            {
                return BadRequest("End time must be after start time");
            }

            if (bookingDto.StartTime < DateTime.UtcNow)
            {
                return BadRequest("Cannot update to a time in the past");
            }

            // Update booking
            existingBooking.StartTime = bookingDto.StartTime;
            existingBooking.EndTime = bookingDto.EndTime;
            existingBooking.Notes = bookingDto.Notes;

            try
            {
                var result = await _bookingService.UpdateBookingAsync(existingBooking);
                if (result)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest("Failed to update booking");
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingBooking = await _bookingService.GetBookingByIdAsync(id);

            if (existingBooking == null)
            {
                return NotFound();
            }

            // Check if the booking belongs to the current user
            if (existingBooking.UserId != userId)
            {
                return Forbid();
            }

            var result = await _bookingService.CancelBookingAsync(id);
            if (result)
            {
                return NoContent();
            }
            else
            {
                return BadRequest("Failed to cancel booking");
            }
        }

        [HttpGet("court/{courtId}/availability")]
        public async Task<ActionResult<CourtAvailabilityResponseDto>> GetCourtAvailability(
            int courtId, 
            [FromQuery] DateTime date)
        {
            // Get all bookings for the court on the specified date
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var bookings = await _bookingService.GetCourtBookingsAsync(courtId, startDate, endDate);
            
            // Get court details
            var court = await _bookingService.GetBookingByIdAsync(bookings.FirstOrDefault()?.Id ?? 0);
            
            // If no bookings exist, we need to get court details another way
            string courtName = "Unknown Court";
            string facilityName = "Unknown Facility";
            
            if (court != null && court.Court != null)
            {
                courtName = court.Court.Name;
                facilityName = court.Court.Facility?.Name ?? "Unknown Facility";
            }

            // Create time slots (e.g., 1-hour slots from 8 AM to 10 PM)
            var timeSlots = new List<TimeSlotDto>();
            var operatingStart = startDate.AddHours(8); // 8 AM
            var operatingEnd = startDate.AddHours(22);  // 10 PM

            for (var slotStart = operatingStart; slotStart < operatingEnd; slotStart = slotStart.AddHours(1))
            {
                var slotEnd = slotStart.AddHours(1);
                
                // Check if this slot overlaps with any existing bookings
                bool isAvailable = !bookings.Any(b => 
                    (b.StartTime < slotEnd && b.EndTime > slotStart) || 
                    (b.StartTime == slotStart && b.EndTime == slotEnd));

                timeSlots.Add(new TimeSlotDto
                {
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    IsAvailable = isAvailable
                });
            }

            var availabilityResponse = new CourtAvailabilityResponseDto
            {
                CourtId = courtId,
                CourtName = courtName,
                FacilityName = facilityName,
                Date = date.Date,
                TimeSlots = timeSlots
            };

            return Ok(availabilityResponse);
        }

        [HttpPost("lock")]
        public async Task<ActionResult<BookingLockResponseDto>> CreateBookingLock(BookingLockRequestDto lockDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Validate lock time
            if (lockDto.StartTime >= lockDto.EndTime)
            {
                return BadRequest("End time must be after start time");
            }

            if (lockDto.StartTime < DateTime.UtcNow)
            {
                return BadRequest("Cannot lock a time in the past");
            }

            try
            {
                var bookingLock = await _bookingService.CreateBookingLockAsync(
                    lockDto.CourtId,
                    userId,
                    lockDto.StartTime,
                    lockDto.EndTime
                );

                // Get court name
                var court = await _bookingService.GetBookingByIdAsync(0); // This is a hack to get court details
                string courtName = "Unknown Court";
                
                if (court != null && court.Court != null)
                {
                    courtName = court.Court.Name;
                }

                var responseDto = new BookingLockResponseDto
                {
                    Id = bookingLock.Id,
                    CourtId = bookingLock.CourtId,
                    CourtName = courtName,
                    StartTime = bookingLock.StartTime,
                    EndTime = bookingLock.EndTime,
                    ExpiresAt = bookingLock.ExpiresAt.Value
                };

                return Ok(responseDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("lock/{id}")]
        public async Task<IActionResult> ReleaseBookingLock(int id)
        {
            var result = await _bookingService.ReleaseBookingLockAsync(id);
            if (result)
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
