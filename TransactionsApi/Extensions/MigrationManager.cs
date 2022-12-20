using FluentMigrator.Runner;
using TransactionsApi.Migrations;

namespace TransactionsApi.Extensions
{
    public static class MigrationManager
    {
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var databaseService = scope.ServiceProvider.GetRequiredService<Database>();
                var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                try
                {
                    databaseService.CreateDatabase("PaymentsDb");

                    migrationService.ListMigrations();
                    migrationService.MigrateUp();

                }
                catch(Exception e)
                {
                    throw;
                }
            }

            return host;
        }
    }
}
