using BadmintonBooking.API.Models;
using BadmintonBooking.API.Data;
using Microsoft.EntityFrameworkCore;
using BadmintonBooking.API.Services.Interfaces;

namespace BadmintonBooking.API.Services
{
    public class CourtService : ICourtService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFacilityService _facilityService;

        public CourtService(ApplicationDbContext context, IFacilityService facilityService)
        {
            _context = context;
            _facilityService = facilityService;
        }

        public async Task<IEnumerable<Court>> CreateCourtsAsync(int facilityId, string baseName, int numberOfCourts, List<PricingConfigurationRequest> pricingConfigurations)
        {
            try
            {
                var createdCourts = new List<Court>();
                
                for (int i = 1; i <= numberOfCourts; i++)
                {
                    var court = new Court
                    {
                        Name = $"{baseName} {i}",
                        FacilityId = facilityId,
                        OwnerId = (await _context.Facilities.FirstOrDefaultAsync(f => f.Id == facilityId)).OwnerId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        PricingConfigurations = new List<PricingConfiguration>()
                    };
                    
                    // Create pricing configurations for each court
                    foreach (var config in pricingConfigurations)
                    {
                        // Create a pricing configuration for each selected day of the week
                        court.PricingConfigurations.Add(new PricingConfiguration
                        {
                            CourtId = court.Id,
                            DayOfWeek = config.DayOfWeek,
                            StartTime = TimeSpan.Parse(config.StartTime),
                            EndTime = TimeSpan.Parse(config.EndTime),
                            Price = config.Price,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    
                    _context.Courts.Add(court);
                    createdCourts.Add(court);
                }
                
                await _context.SaveChangesAsync();
                return createdCourts;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error creating courts: {ex.Message}", ex);
            }
        }

        public async Task<Court> GetCourtAsync(int id)
        {
            try
            {
                var court = await _context.Courts
                    .Include(c => c.Facility)
                    .Include(c => c.PricingConfigurations)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                return court;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error retrieving court with id {id}: {ex.Message}", ex);
            }
        }

        public async Task<Court> GetCourtByIdAsync(int id)
        {
            try
            {
                var court = await _context.Courts
                    .Include(c => c.Facility)
                    .Include(c => c.PricingConfigurations)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                return court;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error retrieving court with id {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Court>> GetCourtsAsync(int facilityId)
        {
            try
            {
                return await _context.Courts
                    .Where(c => c.FacilityId == facilityId)
                    .Include(c => c.PricingConfigurations.OrderBy(p => p.DayOfWeek))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error retrieving courts for facility {facilityId}: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsCourtOwnerAsync(int courtId, Guid userId)
        {
            try
            {
                var court = await _context.Courts
                    .Include(c => c.Facility)
                    .FirstOrDefaultAsync(c => c.Id == courtId);
                
                if (court == null)
                    return false;
                
                return court.Facility.OwnerId == userId;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error checking court ownership: {ex.Message}", ex);
            }
        }

        public async Task<Court> UpdateCourtAsync(int id, string name, List<PricingConfigurationRequest> pricingConfigurations)
        {
            try
            {
                var court = await _context.Courts
                    .Include(c => c.PricingConfigurations)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (court == null)
                    throw new ArgumentException($"Court with id {id} not found");
                
                // Update court name
                court.Name = name;
                court.UpdatedAt = DateTime.UtcNow;
                
                // Remove existing pricing configurations
                if (court.PricingConfigurations != null && court.PricingConfigurations.Any())
                {
                    _context.PricingConfigurations.RemoveRange(court.PricingConfigurations);
                }
                
                // Add new pricing configurations
                court.PricingConfigurations = new List<PricingConfiguration>();
                foreach (var config in pricingConfigurations)
                {
                    court.PricingConfigurations.Add(new PricingConfiguration
                    {
                        CourtId = court.Id,
                        DayOfWeek = config.DayOfWeek,
                        StartTime = TimeSpan.Parse(config.StartTime),
                        EndTime = TimeSpan.Parse(config.EndTime),
                        Price = config.Price,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                
                await _context.SaveChangesAsync();
                
                // Return the updated court with pricing configurations
                return await GetCourtByIdAsync(id);
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error updating court {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteCourtAsync(int id)
        {
            try
            {
                var court = await _context.Courts
                    .Include(c => c.PricingConfigurations)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (court == null)
                    return false;
                
                // Check if bookings exist
                var hasBookings = await _context.Bookings.AnyAsync(b => b.CourtId == id);
                
                if (hasBookings)
                {
                    throw new InvalidOperationException("Cannot delete court with existing bookings");
                }
                
                // Remove pricing configurations first
                if (court.PricingConfigurations != null && court.PricingConfigurations.Any())
                {
                    _context.PricingConfigurations.RemoveRange(court.PricingConfigurations);
                }
                
                _context.Courts.Remove(court);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error deleting court {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> CheckCourtAvailabilityAsync(int courtId, DateTime startTime, DateTime endTime)
        {
            try
            {
                // Check for overlapping bookings
                var overlappingBookings = await _context.Bookings
                    .Where(b => b.CourtId == courtId && 
                           ((b.StartTime <= startTime && b.EndTime > startTime) ||
                            (b.StartTime < endTime && b.EndTime >= endTime) ||
                            (b.StartTime >= startTime && b.EndTime <= endTime)))
                    .AnyAsync();
                
                return !overlappingBookings;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error checking court availability: {ex.Message}", ex);
            }
        }
    }
}
