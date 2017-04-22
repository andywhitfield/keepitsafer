using KeepItSafer.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KeepItSafer.Web.Data
{
    public class SqliteDataContext : DbContext
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<SqliteDataContext> logger;

        public SqliteDataContext(IConfiguration configuration, ILogger<SqliteDataContext> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }
        
        public DbSet<UserAccount> UserAccounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbDataSource = configuration["DbContext:DbPath"];
            logger.LogDebug($"Using DB location: {dbDataSource}");
            optionsBuilder.UseSqlite($"Data Source={dbDataSource}");
        }
    }
}