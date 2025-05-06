using BadmintonBooking.API.Models;

namespace BadmintonBooking.API.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId);
        Task<IEnumerable<Booking>> GetCourtBookingsAsync(int courtId, DateTime startDate, DateTime endDate);
        Task<Booking> GetBookingByIdAsync(int id);
        Task<Booking> CreateBookingAsync(Booking booking);
        Task<bool> UpdateBookingAsync(Booking booking);
        Task<bool> CancelBookingAsync(int id);
        Task<bool> IsCourtAvailableAsync(int courtId, DateTime startTime, DateTime endTime);
        Task<BookingLock> CreateBookingLockAsync(int courtId, Guid userId, DateTime startTime, DateTime endTime);
        Task<bool> ReleaseBookingLockAsync(int lockId);
    }
}
