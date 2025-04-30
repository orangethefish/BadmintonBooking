using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class CreateCourtRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        
        [Required]
        public int FacilityId { get; set; }
    }

    public class UpdateCourtRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }

    public class CourtResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int FacilityId { get; set; }
        public int OwnerId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 