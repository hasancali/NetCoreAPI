using Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Config
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(
            EntityTypeBuilder<User> builder)
        {
            var navigation = builder
                .Metadata.FindNavigation(nameof(User.RefreshTokens));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.OwnsOne(u => u.Address,
                ua =>
                {
                    ua.Property(x => x.Address1).HasColumnName("address_address1");
                    ua.Property(x => x.Address2).HasColumnName("address_address2");
                });
            builder.Property(u => u.CustomFields)
                .HasColumnType("jsonb");
            builder
                .HasMany(u => u.Roles)
                .WithOne(u => u.User)
                .HasForeignKey(u => u.UserId);

        }
    }
}