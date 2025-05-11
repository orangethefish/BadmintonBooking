using BadmintonBooking.API.Models;
using BadmintonBooking.API.Data;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Court> CreateCourtAsync(CreateCourtRequest request, Guid userId)
        {
            try
            {
                // Verify facility exists and user owns it
                var isOwner = await _facilityService.IsFacilityOwnerAsync(request.FacilityId, userId);
                
                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("User is not the owner of this facility");
                }

                // Get the facility to retrieve owner details
                var facility = await _context.Facilities.FindAsync(request.FacilityId);
                if (facility == null)
                {
                    throw new Exception($"Facility with ID {request.FacilityId} not found");
                }

                var court = new Court
                {
                    Name = request.Name,
                    FacilityId = request.FacilityId,
                    OwnerId = facility.OwnerId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PricingConfigurations = request.PricingConfigurations?.Select(pc => new PricingConfiguration
                    {
                        DayOfWeek = pc.DayOfWeek,
                        StartTime = TimeSpan.TryParse(pc.StartTime, out var startTime) ? startTime : TimeSpan.Zero,
                        EndTime = TimeSpan.TryParse(pc.EndTime, out var endTime) ? endTime : TimeSpan.Zero,
                        Price = pc.Price,
                        HourlyRate = pc.HourlyRate,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }).ToList()
                };

                _context.Courts.Add(court);
                await _context.SaveChangesAsync();
                return court;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error creating court: {ex.Message}", ex);
            }
        }

        public async Task<List<Court>> CreateCourtsBatchAsync(BatchCreateCourtRequest request, Guid userId)
        {
            try
            {
                // Verify facility exists and user owns it
                var isOwner = await _facilityService.IsFacilityOwnerAsync(request.FacilityId, userId);
                
                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("User is not the owner of this facility");
                }

                // Get the facility to retrieve owner details
                var facility = await _context.Facilities.FindAsync(request.FacilityId);
                if (facility == null)
                {
                    throw new Exception($"Facility with ID {request.FacilityId} not found");
                }

                var courts = new List<Court>();
                for (int i = 1; i <= request.NumberOfCourts; i++)
                {
                    var court = new Court
                    {
                        Name = $"{request.BaseName} {i}",
                        FacilityId = request.FacilityId,
                        OwnerId = facility.OwnerId, // Set OwnerId from facility
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        PricingConfigurations = request.PricingConfigurations?.Select(pc => new PricingConfiguration
                        {
                            DayOfWeek = pc.DayOfWeek,
                            StartTime = TimeSpan.TryParse(pc.StartTime, out var startTime) ? startTime : TimeSpan.Zero,
                            EndTime = TimeSpan.TryParse(pc.EndTime, out var endTime) ? endTime : TimeSpan.Zero,
                            Price = pc.Price,
                            HourlyRate = pc.HourlyRate,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }).ToList()
                    };
                    courts.Add(court);
                }

                _context.Courts.AddRange(courts);
                await _context.SaveChangesAsync();
                return courts;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error creating courts batch: {ex.Message}", ex);
            }
        }

        public async Task<Court> GetCourtAsync(int id)
        {
            try
            {
                var court = await _context.Courts
                    .Include(c => c.Facility)
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

        public async Task<bool> UpdateCourtAsync(int id, UpdateCourtRequest request)
        {
            try
            {
                var court = await _context.Courts.FindAsync(id);
                
                if (court == null)
                    return false;
                
                court.Name = request.Name ?? court.Name;
                // court.IsActive = request.IsActive ?? court.IsActive;
                court.UpdatedAt = DateTime.UtcNow;
                
                _context.Courts.Update(court);
                await _context.SaveChangesAsync();
                return true;
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
                var court = await _context.Courts.FindAsync(id);
                
                if (court == null)
                    return false;
                
                // Check if bookings exist
                var hasBookings = await _context.Bookings.AnyAsync(b => b.CourtId == id);
                
                if (hasBookings)
                {
                    throw new InvalidOperationException("Cannot delete court with existing bookings");
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
