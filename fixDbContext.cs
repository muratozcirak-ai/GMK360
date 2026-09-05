using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        if (!code.Contains("using GMK360.Core.Entities.System;"))
        {
            code = code.Replace("using GMK360.Core.Entities;", "using GMK360.Core.Entities;\r\nusing GMK360.Core.Entities.System;\r\nusing System.Reflection;\r\nusing System.Linq;\r\nusing System.Threading;\r\nusing System.Threading.Tasks;\r\nusing System.Text.Json;");
        }

        if (!code.Contains("public DbSet<AuditLog> AuditLogs { get; set; }"))
        {
            code = code.Replace("public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)", 
                "public DbSet<AuditLog> AuditLogs { get; set; }\r\n\r\n        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)");
        }

        if (!code.Contains("SetGlobalQueryFilter"))
        {
            string globalFilterMethod = @"
        private void SetGlobalQueryFilter<T>(ModelBuilder builder) where T : BaseEntity
        {
            builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override int SaveChanges()
        {
            HandleSoftDeleteAndAudit();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleSoftDeleteAndAudit();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void HandleSoftDeleteAndAudit()
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)).ToList();
            
            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                    entity.UpdatedAt = DateTime.UtcNow;
                    
                    // Simple Audit for Soft Delete
                    AuditLogs.Add(new AuditLog {
                        ActionType = ""SOFT_DELETE"",
                        EntityName = entry.Entity.GetType().Name,
                        EntityId = entity.Id.ToString(),
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
        }";
            
            code = code.Replace("protected override void OnModelCreating(ModelBuilder builder)", globalFilterMethod + "\r\n\r\n        protected override void OnModelCreating(ModelBuilder builder)");
        }

        if (!code.Contains("SetGlobalQueryFilter<"))
        {
            string reflectionInvoke = @"
            base.OnModelCreating(builder);
            
            var method = typeof(ApplicationDbContext).GetMethod(nameof(SetGlobalQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    method.MakeGenericMethod(entityType.ClrType).Invoke(this, new object[] { builder });
                }
            }";
            
            code = code.Replace("base.OnModelCreating(builder);", reflectionInvoke);
        }

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Updated DbContext with SoftDelete and Audit.");
    }
}
