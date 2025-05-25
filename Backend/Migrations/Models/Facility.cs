using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class Facility
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Address { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        [Required]
        public Guid OwnerId { get; set; }
        public User Owner { get; set; }
        
        public ICollection<Court> Courts { get; set; }
        [StringLength(200)]
        public string? MapsUrl { get; set; }
        [StringLength(100)]
        public string? CourtLongitude { get; set; }
        [StringLength(100)]
        public string? CourtLatitude { get; set; }
        [StringLength(36)]
        public string? PlaceId { get; set; }
    }
} 