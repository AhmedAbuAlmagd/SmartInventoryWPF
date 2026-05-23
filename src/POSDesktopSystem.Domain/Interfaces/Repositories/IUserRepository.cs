using POSDesktopSystem.Domain.Entities;
using System.Threading.Tasks;

namespace POSDesktopSystem.Domain.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
}
