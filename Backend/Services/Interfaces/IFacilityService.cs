using BadmintonBooking.API.Models;

namespace BadmintonBooking.API.Services.Interfaces
{
    public interface IFacilityService
    {
        Task<Facility> CreateFacilityAsync(CreateFacilityRequest request, Guid userId);
        Task<Facility> GetFacilityAsync(int id);
        Task<IEnumerable<Facility>> GetUserFacilitiesAsync(Guid userId);
        Task<bool> IsFacilityOwnerAsync(int facilityId, Guid userId);
        Task<bool> UpdateFacilityAsync(int id, UpdateFacilityRequest request);
        Task<bool> DeleteFacilityAsync(int id);
        Task<ResolveUrlResponse> ResolveUrlAsync(ResolveUrlRequest request);
    }
}
