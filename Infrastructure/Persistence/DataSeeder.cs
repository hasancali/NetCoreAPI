using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities.Identity;
using Domain.Enums;
using Domain.ValueObjects;

namespace Infrastructure.Persistence
{
    public class DataSeeder
    {
        public static async Task SeedData(
            ApplicationDbContext context,
            IPasswordHasher passwordHasher)
        {
            if (!context.Users.Any())
            {

                var salt = Guid.NewGuid().ToByteArray();
                var user1 = new User(
                    "Hasan",
                    "Çalı",
                    "info@hasancali.dev",
                    "hasan",
                    string.Empty,
                    passwordHasher.Hash(
                        "test",
                        salt),
                    salt,
                    new Address()
                );

                user1.CustomFields = new[] { new CustomField("Age", "25") };

                await context.Users.AddAsync(user1);

                var role1 = new Role("Base User");
                role1.AddPermissions(new List<Permissions> {Permissions.ReadUsers});
                await context.Roles.AddAsync(role1);

                var userRole1 = new UserRole(user1, role1);
                await context.UserRoles.AddAsync(userRole1);
                
                var user2 = new User(
                    "Hasan",
                    "Çalı",
                    "hasancali@hasancali.dev",
                    "hasancali",
                    string.Empty,
                    passwordHasher.Hash(
                        "test",
                        salt),
                    salt,
                    new Address()
                );
                await context.Users.AddAsync(user2);

                var role2 = new Role("Super User");
                role2.AddPermissions(new List<Permissions> {Permissions.SuperUser});
                await context.Roles.AddAsync(role2);

                var userRole2 = new UserRole(user2, role2);
                await context.UserRoles.AddAsync(userRole2);
                
                await context.SaveChangesAsync();
            }
        }
    }
}