using Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Config
{
    public class UserRoleConfig : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(
            EntityTypeBuilder<UserRole> builder)
        {
            builder
                .ToTable("userroles")
                .HasKey(r => new {r.UserId, r.RoleId});
        }
    }
}