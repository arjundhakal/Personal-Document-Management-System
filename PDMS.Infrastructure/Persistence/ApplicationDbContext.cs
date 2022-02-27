using PDMS.Application.Interfaces;
using PDMS.Domain.Common;
using PDMS.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace PDMS.Infrastructure
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<DocumentDetail> DocumentDetails { get; set; }
        public DbSet<Session> Sessions { get; set; }

        public async Task<int> SaveChangesAsync()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;

                }
            }

            return await base.SaveChangesAsync();
        }

        public new DbSet<T> Set<T>() where T : BaseEntity
        {
            return base.Set<T>();
        }
    }
}
