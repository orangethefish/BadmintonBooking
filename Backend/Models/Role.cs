using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        
        [MaxLength(200)]
        public string Description { get; set; }
        
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; }
    }
} 