namespace CleanTemplate.Persistence.Db
{
    using Common.Utilities;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System.Threading;
    using System.Threading.Tasks;

    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public AppDbContext() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entitiesAssembly = typeof(IEntity).Assembly;
            var persistenceAssembly = typeof(AppDbContext).Assembly;

            modelBuilder.RegisterAllEntities<IEntity>(entitiesAssembly);
            // Apply configurations from the Persistence assembly (where configurations live)
            modelBuilder.ApplyConfigurationsFromAssembly(persistenceAssembly);
            modelBuilder.AddPluralizingTableNameConvention();
        }

        public async Task<int> ExecuteSqlRawAsync(string query, CancellationToken cancellationToken)
        {
            var result = await base.Database.ExecuteSqlRawAsync(query, cancellationToken);
            return result;
        }

        public async Task<int> ExecuteSqlRawAsync(string query) => await ExecuteSqlRawAsync(query, CancellationToken.None);
    }
}
