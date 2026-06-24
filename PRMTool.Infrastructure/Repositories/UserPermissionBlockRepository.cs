using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using PRMTool.Infrastructure.Data;

namespace PRMTool.Infrastructure.Repositories
{
    public class UserPermissionBlockRepository : IUserPermissionBlockRepository
    {
        private readonly AppDbContext _context;

        public UserPermissionBlockRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsBlockedAsync(int userId, string permissionCode)
        {
            return await _context.UserPermissionBlocks
                .AnyAsync(b => b.UserId == userId && b.Permission!.PermissionCode == permissionCode);
        }

        public async Task AddAsync(UserPermissionBlock block)
        {
            await _context.UserPermissionBlocks.AddAsync(block);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(int userId, string permissionCode)
        {
            var block = await _context.UserPermissionBlocks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Permission!.PermissionCode == permissionCode);

            if (block != null)
            {
                _context.UserPermissionBlocks.Remove(block);
                await _context.SaveChangesAsync();
            }
        }
    }
}
