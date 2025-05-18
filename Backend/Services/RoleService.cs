using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BadmintonBooking.API.Data;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services.Interfaces;

namespace BadmintonBooking.API.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService _logger;

        public RoleService(ApplicationDbContext context, ILoggerService logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task EnsureRolesCreatedAsync()
        {
            try
            {
                _logger.Info("Checking if default roles exist");
                
                // Define the default roles
                var defaultRoles = new List<Role>
                {
                    new Role 
                    { 
                        Id = Guid.NewGuid(), 
                        Name = "Admin", 
                        Description = "Administrator with full system access" 
                    },
                    new Role 
                    { 
                        Id = Guid.NewGuid(), 
                        Name = "Owner", 
                        Description = "Facility owner with management capabilities" 
                    },
                    new Role 
                    { 
                        Id = Guid.NewGuid(), 
                        Name = "User", 
                        Description = "Regular user with booking capabilities" 
                    }
                };

                // Check each role and add if it doesn't exist
                foreach (var role in defaultRoles)
                {
                    var existingRole = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Name == role.Name);
                        
                    if (existingRole == null)
                    {
                        _logger.Info($"Creating default role: {role.Name}");
                        _context.Roles.Add(role);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.Info("Default roles verified/created successfully");
            }
            catch (Exception ex)
            {
                _logger.Error("Error ensuring roles exist", ex);
                throw;
            }
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task<bool> AddUserToRoleAsync(Guid userId, string roleName)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.Warning($"AddUserToRole failed: User not found with ID: {userId}");
                    return false;
                }

                var role = await GetRoleByNameAsync(roleName);
                if (role == null)
                {
                    _logger.Warning($"AddUserToRole failed: Role not found: {roleName}");
                    return false;
                }

                // Check if the user already has this role
                var existingUserRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
                    
                if (existingUserRole != null)
                {
                    // User already has this role
                    return true;
                }

                // Add the role to the user
                var userRole = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RoleId = role.Id
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
                
                _logger.Info($"Added role {roleName} to user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error adding user {userId} to role {roleName}", ex);
                return false;
            }
        }

        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
                
            return userRoles;
        }
    }
} 