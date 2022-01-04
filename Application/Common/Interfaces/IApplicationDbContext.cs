using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Domain.Entities.Audit.Audit> Audits { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}