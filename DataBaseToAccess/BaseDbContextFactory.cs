using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.IO;

namespace DataBaseToAccess
{
    public class BaseDbContextFactory : IDesignTimeDbContextFactory<BaseDbContext>
    {
        public BaseDbContext CreateDbContext(string[] args)
        {
            
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<BaseDbContext>();

            
            var connectionString = configuration.GetConnectionString("DataBaseConnection");
            optionsBuilder.UseNpgsql(connectionString);

            return new BaseDbContext(optionsBuilder.Options);
        }
    }
}
