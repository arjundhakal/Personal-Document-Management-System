using PDMS.Application.Interfaces;
using PDMS.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PDMS.Application.Repositories
{
    public class DatabaseRepository<T> : IDatabaseRepository<T> where T : BaseEntity
    {

        private readonly IApplicationDbContext _dbContext;
        private readonly DbSet<T> _entities;

        public DatabaseRepository(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _entities = dbContext.Set<T>();
        }


        public async Task<int> CountAsync()
        {
            return await _entities.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _entities.CountAsync(predicate);
        }

        public async Task Create(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _entities.Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            if (id == 0)
                throw new ArgumentNullException(nameof(id));

            T entity = _entities.SingleOrDefault(s => s.Id.Equals(id));

            if (entity == null)
                throw new Exception("Unable to delete record as it does not exist");

            _entities.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> ReadAsync()
        {
            return await _entities.ToListAsync();
        }

        public async Task<IEnumerable<T>> ReadAsync(Expression<Func<T, bool>> predicate)
        {
            return await _entities.Where(predicate).ToListAsync();
        }

        public async Task<T> SingleOrDefaultAsync(int id)
        {
            return await _entities.SingleOrDefaultAsync(s => s.Id == id);
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _entities.SingleOrDefaultAsync(predicate);
        }

        public async Task Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbContext.SaveChangesAsync();
        }
    }
}
