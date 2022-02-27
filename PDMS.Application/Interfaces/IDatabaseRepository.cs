using PDMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PDMS.Application.Interfaces
{
    public interface IDatabaseRepository<T> where T : BaseEntity
    {
        Task Create(T entity);
        Task<T> SingleOrDefaultAsync(int id);
        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> ReadAsync();
        Task<IEnumerable<T>> ReadAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task Update(T entity);
        Task Delete(int id);
    }
}