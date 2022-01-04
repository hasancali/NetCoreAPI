using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Audit;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly AuditSettings _auditSettings;

        public ApplicationDbContext(
            DbContextOptions options,
            ICurrentUserService currentUserService,
            ILoggerFactory loggerFactory,
            IOptions<AuditSettings> auditSettings)
            : base(options)
        {
            _currentUserService = currentUserService;
            _loggerFactory = loggerFactory;
            _auditSettings = auditSettings.Value;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Domain.Entities.Audit.Audit> Audits { get; set; }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(_loggerFactory);
        }

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            var auditEntries = await OnBeforeSaveChanges();
            UpdateAuditFieldsOnEntities();
            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChanges(auditEntries);
            return result;
        }

        public override int SaveChanges()
        {
            return SaveChangesAsync().GetAwaiter().GetResult();
        }

        async Task<IEnumerable<(EntityEntry EntityEntry, Domain.Entities.Audit.Audit Audit)>> OnBeforeSaveChanges()
        {
            if (!_auditSettings.Enabled)
                return null;

            ChangeTracker.DetectChanges();
            var entitiesToTrack = ChangeTracker.Entries().Where(
                e => !(e.Entity is Domain.Entities.Audit.Audit) && e.State != EntityState.Detached &&
                     e.State != EntityState.Unchanged);

            foreach (var entityEntry in entitiesToTrack.Where(e => !e.Properties.Any(p => p.IsTemporary)))
            {
                var auditExcludedProps = entityEntry.Entity.GetType()
                    .GetProperties()
                    .Where(
                        p => p.GetCustomAttributes(
                            typeof(DoNotAudit),
                            false).Any())
                    .Select(p => p.Name)
                    .ToList();


                await Audits.AddRangeAsync(
                    new Domain.Entities.Audit.Audit
                    {
                        Table = entityEntry.Metadata.GetTableName(),
                        Date = DateTime.Now.ToUniversalTime(),
                        UserId = _currentUserService.Id,
                        UserName = _currentUserService.Name,
                        KeyValues = JsonSerializer.Serialize(
                            entityEntry.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToDictionary(
                                p => p.Metadata.Name,
                                p => p.CurrentValue)),
                        NewValues = JsonSerializer.Serialize(
                            entityEntry.Properties.Where(
                                    p => entityEntry.State == EntityState.Added ||
                                         entityEntry.State == EntityState.Modified &&
                                         !auditExcludedProps.Contains(p.Metadata.Name))
                                .ToDictionary(
                                    p => p.Metadata.Name,
                                    p => p.CurrentValue)),
                        OldValues = JsonSerializer.Serialize(
                            entityEntry.Properties.Where(
                                    p => entityEntry.State == EntityState.Deleted ||
                                         entityEntry.State == EntityState.Modified &&
                                         !auditExcludedProps.Contains(p.Metadata.Name))
                                .ToDictionary(
                                    p => p.Metadata.Name,
                                    p => p.OriginalValue))
                    });
            }

            var returnList = new List<(EntityEntry EntityEntry, Domain.Entities.Audit.Audit Audit)>();
            foreach (var entityEntry in entitiesToTrack.Where(e => e.Properties.Any(p => p.IsTemporary)))
            {
                var auditExcludedProps = entityEntry.Entity.GetType()
                    .GetProperties()
                    .Where(
                        p => p.GetCustomAttributes(
                            typeof(DoNotAudit),
                            false).Any())
                    .Select(p => p.Name)
                    .ToList();

                returnList.Add(
                    (entityEntry,
                        new Domain.Entities.Audit.Audit
                        {
                            Table = entityEntry.Metadata.GetTableName(),
                            Date = DateTime.Now.ToUniversalTime(),
                            UserId = _currentUserService.Id,
                            UserName = _currentUserService.Name,
                            NewValues = JsonSerializer.Serialize(
                                entityEntry.Properties.Where(
                                    p => !p.Metadata.IsPrimaryKey() &&
                                         !auditExcludedProps.Contains(p.Metadata.Name)).ToDictionary(
                                    p => p.Metadata.Name,
                                    p => p.CurrentValue))
                        }
                    ));
            }

            return returnList;
        }

        private async Task OnAfterSaveChanges(
            IEnumerable<(EntityEntry EntityEntry, Domain.Entities.Audit.Audit Audit)> auditEntries)
        {
            if (!_auditSettings.Enabled)
                return;

            if (auditEntries != null && auditEntries.Any())
            {
                foreach (var auditEntry in auditEntries)
                {
                    auditEntry.Audit.KeyValues = JsonSerializer.Serialize(
                        auditEntry.EntityEntry.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToDictionary(
                            p => p.Metadata.Name,
                            p => p.CurrentValue));

                    Audits.Add(auditEntry.Audit);
                }

                await SaveChangesAsync();
            }

            await Task.CompletedTask;
        }

        private void UpdateAuditFieldsOnEntities()
        {
            // get entries that are being Added or Updated
            var modifiedEntries = ChangeTracker.Entries()
                .Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entry in modifiedEntries)
            {
                var entity = entry.Entity as Entity;

                if (entity != null)
                {
                    var now = DateTime.Now;
                    var userId = _currentUserService.Id;

                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedBy = userId;
                        entity.CreatedAt = now;
                    }

                    entity.UpdatedBy = userId;
                    entity.UpdatedAt = now;
                }
            }
        }
    }
}