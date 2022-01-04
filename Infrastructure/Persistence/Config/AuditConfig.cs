using Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Config
{
    public class AuditConfig  : IEntityTypeConfiguration<Audit>
    {
        public void Configure(
            EntityTypeBuilder<Audit> builder)
        {
            builder.Property(x => x.NewValues)
                .HasColumnType("jsonb");
            builder.Property(x => x.OldValues)
                .HasColumnType("jsonb");
        }
    }
}