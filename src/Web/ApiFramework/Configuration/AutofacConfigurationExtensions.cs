using Autofac;
using CleanTemplate.Common;
using CleanTemplate.Common.General;
using CleanTemplate.Domain.Entities;
using CleanTemplate.Domain.IRepositories;
using CleanTemplate.Persistence.Db;
using CleanTemplate.Persistence.Repositories;

namespace CleanTemplate.ApiFramework.Configuration
{
    public static class AutofacConfigurationExtensions
    {
        public static void RegisterServices(this ContainerBuilder containerBuilder)
        {
            //RegisterType > As > Liftetime
            containerBuilder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

            containerBuilder.RegisterGeneric(typeof(EfReadOnlyRepository<>)).As(typeof(IReanOnlyRepository<>)).InstancePerLifetimeScope();

            var commonAssembly = typeof(SiteSettings).Assembly;
            var entitiesAssembly = typeof(IEntity).Assembly;
            var dataAssembly = typeof(AppDbContext).Assembly;
            // لم يعد هناك JwtService محلي بعد التحول إلى Tamkeen.IdentityService.
            containerBuilder.RegisterAssemblyTypes(commonAssembly, entitiesAssembly, dataAssembly)
                .AssignableTo<IScopedDependency>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterAssemblyTypes(commonAssembly, entitiesAssembly, dataAssembly)
                .AssignableTo<ITransientDependency>()
                .AsImplementedInterfaces()
                .InstancePerDependency();

            containerBuilder.RegisterAssemblyTypes(commonAssembly, entitiesAssembly, dataAssembly)
                .AssignableTo<ISingletonDependency>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
