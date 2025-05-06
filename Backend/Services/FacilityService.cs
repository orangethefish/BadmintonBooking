using BadmintonBooking.API.Models;
using BadmintonBooking.API.Data;
using Microsoft.EntityFrameworkCore;

namespace BadmintonBooking.API.Services
{
    public class FacilityService : IFacilityService
    {
        private readonly ApplicationDbContext _context;

        public FacilityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Facility> CreateFacilityAsync(CreateFacilityRequest request, Guid userId)
        {
            try
            {
                var facility = new Facility
                {
                    Name = request.Name,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    Description = request.Description,
                    OwnerId = userId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Facilities.Add(facility);
                await _context.SaveChangesAsync();
                return facility;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error creating facility: {ex.Message}", ex);
            }
        }

        public async Task<Facility> GetFacilityAsync(int id)
        {
            try
            {
                var facility = await _context.Facilities
                    .Include(f => f.Courts)
                    .FirstOrDefaultAsync(f => f.Id == id);
                
                return facility;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error retrieving facility with id {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Facility>> GetUserFacilitiesAsync(Guid userId)
        {
            try
            {
                return await _context.Facilities
                    .Where(f => f.OwnerId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error retrieving facilities for user {userId}: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsFacilityOwnerAsync(int facilityId, Guid userId)
        {
            try
            {
                var facility = await _context.Facilities
                    .FirstOrDefaultAsync(f => f.Id == facilityId && f.OwnerId == userId);
                
                return facility != null;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error checking facility ownership: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateFacilityAsync(int id, UpdateFacilityRequest request)
        {
            try
            {
                var facility = await _context.Facilities.FindAsync(id);
                
                if (facility == null)
                    return false;
                
                facility.Name = request.Name ?? facility.Name;
                facility.Address = request.Address ?? facility.Address;
                facility.PhoneNumber = request.PhoneNumber ?? facility.PhoneNumber;
                facility.Description = request.Description ?? facility.Description;
                // facility.IsActive = request.IsActive ?? facility.IsActive;
                facility.UpdatedAt = DateTime.UtcNow;
                
                _context.Facilities.Update(facility);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error updating facility {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFacilityAsync(int id)
        {
            try
            {
                var facility = await _context.Facilities.FindAsync(id);
                
                if (facility == null)
                    return false;
                
                // Check if courts exist
                var hasCourts = await _context.Courts.AnyAsync(c => c.FacilityId == id);
                
                if (hasCourts)
                {
                    throw new InvalidOperationException("Cannot delete facility with existing courts");
                }
                
                _context.Facilities.Remove(facility);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error deleting facility {id}: {ex.Message}", ex);
            }
        }
    }
}
