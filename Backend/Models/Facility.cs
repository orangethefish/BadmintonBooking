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
        
        public ICollection<Court> Courts { get; set; }
    }
} 