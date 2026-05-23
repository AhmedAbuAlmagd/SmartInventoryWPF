using POSDesktopSystem.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POSDesktopSystem.Domain.Interfaces.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
