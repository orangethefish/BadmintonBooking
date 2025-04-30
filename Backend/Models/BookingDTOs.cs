using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class BookingRequestDto
    {
        [Required]
        public int CourtId { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        public string Notes { get; set; }
    }

    public class BookingResponseDto
    {
        public int Id { get; set; }
        public int CourtId { get; set; }
        public string CourtName { get; set; }
        public string FacilityName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class BookingLockRequestDto
    {
        [Required]
        public int CourtId { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
    }

    public class BookingLockResponseDto
    {
        public int Id { get; set; }
        public int CourtId { get; set; }
        public string CourtName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class CourtAvailabilityRequestDto
    {
        [Required]
        public int CourtId { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
    }

    public class TimeSlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CourtAvailabilityResponseDto
    {
        public int CourtId { get; set; }
        public string CourtName { get; set; }
        public string FacilityName { get; set; }
        public DateTime Date { get; set; }
        public List<TimeSlotDto> TimeSlots { get; set; }
    }
}
