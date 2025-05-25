using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BadmintonBooking.API.Models
{
    public class RolePermission
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid RoleId { get; set; }
        
        [Required]
        public Guid PermissionId { get; set; }
        
        [ForeignKey("RoleId")]
        public Role Role { get; set; }
        
        [ForeignKey("PermissionId")]
        public Permission Permission { get; set; }
    }
} 