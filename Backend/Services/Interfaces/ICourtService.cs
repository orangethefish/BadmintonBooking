using BadmintonBooking.API.Models;

namespace BadmintonBooking.API.Services
{
    public interface ICourtService
    {
        Task<Court> GetCourtAsync(int id);
        Task<IEnumerable<Court>> GetCourtsAsync(int facilityId);
        Task<bool> IsCourtOwnerAsync(int courtId, Guid userId);
        Task<bool> UpdateCourtAsync(int id, UpdateCourtRequest request);
        Task<bool> DeleteCourtAsync(int id);
        Task<bool> CheckCourtAvailabilityAsync(int courtId, DateTime startTime, DateTime endTime);
        Task<IEnumerable<Court>> CreateCourtsAsync(int facilityId, string baseName, int numberOfCourts, List<PricingConfigurationRequest> pricingConfigurations);
    }
}
