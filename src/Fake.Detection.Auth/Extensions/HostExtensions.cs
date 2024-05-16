using FluentMigrator.Runner;

namespace Fake.Detection.Auth.Extensions;

public static class HostExtensions
{
    public static IHost MigrateUp(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
        return app;
    }
}