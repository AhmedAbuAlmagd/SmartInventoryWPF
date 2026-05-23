using Microsoft.EntityFrameworkCore;
using POSDesktopSystem.Domain.Entities;
using POSDesktopSystem.Domain.Interfaces.Repositories;
using POSDesktopSystem.Infrastructure.Data;
using System.Threading.Tasks;

namespace POSDesktopSystem.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }
}
