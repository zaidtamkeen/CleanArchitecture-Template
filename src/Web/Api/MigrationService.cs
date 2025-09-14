using CleanTemplate.Persistence.Db;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CleanTemplate.Api
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;

    public interface IMigrationService
    {
        void ApplyMigrations();
    }

    public class MigrationService : IMigrationService
    {
        private readonly IServiceProvider _serviceProvider;

        public MigrationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ApplyMigrations()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                // إذا لم توجد أي Migrations في المشروع، فعند التطوير قم بإنشاء المخطط مباشرةً.
                // هذا يمنع أخطاء مثل: Invalid object name 'Products'.
                var hasMigrations = context.Database.GetMigrations().Any();
                if (!hasMigrations)
                {
                    context.Database.EnsureCreated();
                }
                else
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating or creating the database.");
            }
        }
    }
}
