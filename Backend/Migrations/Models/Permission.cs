using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class Permission
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(200)]
        public string Description { get; set; }
        
        public ICollection<RolePermission> RolePermissions { get; set; }
    }
} 