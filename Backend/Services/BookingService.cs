using BadmintonBooking.API.Data;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BadmintonBooking.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService _logger;

        public BookingService(ApplicationDbContext context, ILoggerService logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            try
            {
                _logger.Info("Retrieving all bookings");
                var bookings = await _context.Bookings
                    .Include(b => b.Court)
                    .Include(b => b.User)
                    .OrderByDescending(b => b.StartTime)
                    .ToListAsync();
                _logger.Info($"Successfully retrieved {bookings.Count} bookings");
                return bookings;
            }
            catch (Exception ex)
            {
                _logger.Error("Error retrieving all bookings", ex);
                return new List<Booking>();
            }
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId)
        {
            try
            {
                _logger.Info($"Retrieving bookings for user ID: {userId}");
                var bookings = await _context.Bookings
                    .Include(b => b.Court)
                    .ThenInclude(c => c.Facility)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.StartTime)
                    .ToListAsync();
                _logger.Info($"Successfully retrieved {bookings.Count} bookings for user ID: {userId}");
                return bookings;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error retrieving bookings for user ID: {userId}", ex);
                return new List<Booking>();
            }
        }

        public async Task<IEnumerable<Booking>> GetCourtBookingsAsync(int courtId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Info($"Retrieving bookings for court ID: {courtId} between {startDate} and {endDate}");
                var bookings = await _context.Bookings
                    .Where(b => b.CourtId == courtId && 
                               b.StartTime >= startDate && 
                               b.EndTime <= endDate)
                    .OrderBy(b => b.StartTime)
                    .ToListAsync();
                _logger.Info($"Successfully retrieved {bookings.Count} bookings for court ID: {courtId}");
                return bookings;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error retrieving bookings for court ID: {courtId}", ex);
                return new List<Booking>();
            }
        }

        public async Task<Booking> GetBookingByIdAsync(int id)
        {
            try
            {
                _logger.Info($"Retrieving booking with ID: {id}");
                var booking = await _context.Bookings
                    .Include(b => b.Court)
                    .ThenInclude(c => c.Facility)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == id);
                
                if (booking == null)
                {
                    _logger.Warning($"Booking with ID: {id} not found");
                }
                else
                {
                    _logger.Info($"Successfully retrieved booking with ID: {id}");
                }
                
                return booking;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error retrieving booking with ID: {id}", ex);
                return null;
            }
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            try
            {
                _logger.Info($"Creating new booking for court ID: {booking.CourtId} and user ID: {booking.UserId}");
                
                // Check if the court is available
                bool isAvailable = await IsCourtAvailableAsync(booking.CourtId, booking.StartTime, booking.EndTime);
                if (!isAvailable)
                {
                    _logger.Warning($"Court ID: {booking.CourtId} is not available for the selected time period");
                    throw new InvalidOperationException("The court is not available for the selected time period.");
                }

                // Calculate price based on pricing configuration
                var pricingConfig = await _context.PricingConfigurations
                    .Where(p => p.CourtId == booking.CourtId)
                    .FirstOrDefaultAsync();

                if (pricingConfig == null)
                {
                    _logger.Warning($"No pricing configuration found for court ID: {booking.CourtId}");
                    throw new InvalidOperationException("No pricing configuration found for this court.");
                }

                // Calculate duration in hours
                var duration = (booking.EndTime - booking.StartTime).TotalHours;
                booking.TotalPrice = (decimal)(duration * (double)pricingConfig.HourlyRate);

                // Set booking status and timestamps
                booking.Status = "Confirmed";
                booking.CreatedAt = DateTime.UtcNow;

                // Add booking to database
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Release any locks for this booking
                await _context.BookingLocks
                    .Where(l => l.CourtId == booking.CourtId && 
                               l.UserId == booking.UserId && 
                               l.StartTime == booking.StartTime && 
                               l.EndTime == booking.EndTime)
                    .ExecuteDeleteAsync();

                _logger.Info($"Successfully created booking with ID: {booking.Id}");
                return booking;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating booking for court ID: {booking.CourtId} and user ID: {booking.UserId}", ex);
                if (ex is InvalidOperationException)
                    throw;
                throw new InvalidOperationException($"Failed to create booking: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            try
            {
                _logger.Info($"Updating booking with ID: {booking.Id}");
                var existingBooking = await _context.Bookings.FindAsync(booking.Id);
                if (existingBooking == null)
                {
                    _logger.Warning($"Booking with ID: {booking.Id} not found for update");
                    return false;
                }

                // If changing time, check availability
                if (existingBooking.StartTime != booking.StartTime || existingBooking.EndTime != booking.EndTime)
                {
                    _logger.Info($"Checking court availability for updated time slot");
                    var conflictingBookings = await _context.Bookings
                        .Where(b => b.CourtId == booking.CourtId &&
                                   b.Id != booking.Id &&
                                   ((b.StartTime < booking.EndTime && b.EndTime > booking.StartTime) ||
                                    (b.StartTime == booking.StartTime && b.EndTime == booking.EndTime)))
                        .AnyAsync();

                    if (conflictingBookings)
                    {
                        _logger.Warning($"Court ID: {booking.CourtId} is not available for the updated time period");
                        throw new InvalidOperationException("The court is not available for the selected time period.");
                    }
                }

                // Update booking properties
                existingBooking.StartTime = booking.StartTime;
                existingBooking.EndTime = booking.EndTime;
                existingBooking.Status = booking.Status;
                existingBooking.Notes = booking.Notes;
                existingBooking.UpdatedAt = DateTime.UtcNow;

                // Recalculate price if necessary
                if (existingBooking.StartTime != booking.StartTime || existingBooking.EndTime != booking.EndTime)
                {
                    var pricingConfig = await _context.PricingConfigurations
                        .Where(p => p.CourtId == booking.CourtId)
                        .FirstOrDefaultAsync();

                    if (pricingConfig != null)
                    {
                        var duration = (booking.EndTime - booking.StartTime).TotalHours;
                        existingBooking.TotalPrice = (decimal)(duration * (double)pricingConfig.HourlyRate);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.Info($"Successfully updated booking with ID: {booking.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating booking with ID: {booking.Id}", ex);
                if (ex is InvalidOperationException)
                    throw;
                throw new InvalidOperationException($"Failed to update booking: {ex.Message}", ex);
            }
        }

        public async Task<bool> CancelBookingAsync(int id)
        {
            try
            {
                _logger.Info($"Cancelling booking with ID: {id}");
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    _logger.Warning($"Booking with ID: {id} not found for cancellation");
                    return false;
                }

                booking.Status = "Cancelled";
                booking.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.Info($"Successfully cancelled booking with ID: {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error cancelling booking with ID: {id}", ex);
                throw new InvalidOperationException($"Failed to cancel booking: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsCourtAvailableAsync(int courtId, DateTime startTime, DateTime endTime)
        {
            try
            {
                _logger.Info($"Checking availability for court ID: {courtId} between {startTime} and {endTime}");
                var overlappingBookings = await _context.Bookings
                    .Where(b => b.CourtId == courtId &&
                               b.Status != "Cancelled" &&
                               ((b.StartTime < endTime && b.EndTime > startTime) ||
                                (b.StartTime == startTime && b.EndTime == endTime)))
                    .AnyAsync();

                var isAvailable = !overlappingBookings;
                _logger.Info($"Court ID: {courtId} availability check result: {(isAvailable ? "Available" : "Not Available")}");
                return isAvailable;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking availability for court ID: {courtId}", ex);
                return false;
            }
        }

        public async Task<BookingLock> CreateBookingLockAsync(int courtId, int userId, DateTime startTime, DateTime endTime)
        {
            try
            {
                _logger.Info($"Creating booking lock for court ID: {courtId}, user ID: {userId}");
                var bookingLock = new BookingLock
                {
                    CourtId = courtId,
                    UserId = userId,
                    StartTime = startTime,
                    EndTime = endTime,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                };

                _context.BookingLocks.Add(bookingLock);
                await _context.SaveChangesAsync();
                _logger.Info($"Successfully created booking lock with ID: {bookingLock.Id}");
                return bookingLock;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating booking lock for court ID: {courtId}, user ID: {userId}", ex);
                throw new InvalidOperationException($"Failed to create booking lock: {ex.Message}", ex);
            }
        }

        public async Task<bool> ReleaseBookingLockAsync(int lockId)
        {
            try
            {
                _logger.Info($"Releasing booking lock with ID: {lockId}");
                var bookingLock = await _context.BookingLocks.FindAsync(lockId);
                if (bookingLock == null)
                {
                    _logger.Warning($"Booking lock with ID: {lockId} not found for release");
                    return false;
                }

                _context.BookingLocks.Remove(bookingLock);
                await _context.SaveChangesAsync();
                _logger.Info($"Successfully released booking lock with ID: {lockId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error releasing booking lock with ID: {lockId}", ex);
                throw new InvalidOperationException($"Failed to release booking lock: {ex.Message}", ex);
            }
        }
    }
}
