using Fake.Detection.Auth.Configure;
using Fake.Detection.Auth.Contexts;
using Fake.Detection.Auth.Repositories;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fake.Detection.Auth.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDal(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<DalOptions>(config.GetSection(nameof(DalOptions)));

        services
            .AddDbContexts()
            .AddMigrations()
            .AddRepositories();

        return services;
    }

    private static IServiceCollection AddDbContexts(this IServiceCollection services)
    {
        services.AddDbContext<UserContext>((s, options) =>
        {
            var cfg = s.GetRequiredService<IOptions<DalOptions>>();
            options.UseLazyLoadingProxies().UseNpgsql(cfg.Value.ConnectionString);
        }, ServiceLifetime.Singleton);

        return services;
    }

    private static IServiceCollection AddMigrations(this IServiceCollection services)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb.AddPostgres()
                .WithGlobalConnectionString(s =>
                {
                    var cfg = s.GetRequiredService<IOptions<DalOptions>>();
                    return cfg.Value.ConnectionString;
                })
                .ScanIn(typeof(ServiceCollectionExtensions).Assembly).For.Migrations()
            )
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, UserRepository>();

        return services;
    }
}