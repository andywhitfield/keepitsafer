using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KeepItSafer.Web.Data
{
    // used by the migrations tool only
    public class SqliteDataContextFactory : IDesignTimeDbContextFactory<SqliteDataContext>
    {
        public SqliteDataContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            return new SqliteDataContext(configuration, new LoggerFactory().CreateLogger<SqliteDataContext>());
        }
    }
}