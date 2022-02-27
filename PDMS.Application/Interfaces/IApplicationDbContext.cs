using PDMS.Domain.Common;
using PDMS.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace PDMS.Application.Interfaces
{
	public interface IApplicationDbContext
	{
		DbSet<User> Users { get; set; }
		DbSet<DocumentDetail> DocumentDetails { get; set; }
		DbSet<Session> Sessions { get; set; }
		Task<int> SaveChangesAsync();
		DbSet<T> Set<T>() where T : BaseEntity;
	}
}