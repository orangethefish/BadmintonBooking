using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BadmintonBooking.API.Models;

namespace BadmintonBooking.API.Services.Interfaces
{
    public interface IRoleService
    {
        Task EnsureRolesCreatedAsync();
        Task<List<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByNameAsync(string roleName);
        Task<bool> AddUserToRoleAsync(Guid userId, string roleName);
        Task<List<string>> GetUserRolesAsync(Guid userId);
    }
} 