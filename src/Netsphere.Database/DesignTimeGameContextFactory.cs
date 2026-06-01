using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;
using Netsphere.Database.Configuration;
using Hjson;
using Netsphere.Common.Configuration;

namespace Netsphere.Database
{
    public class DesignTimeGameContextFactory : IDesignTimeDbContextFactory<GameContext>
    {
        public GameContext CreateDbContext(string[] args)
        {
            var config = HjsonValue.Load("config.hjson")
                ["Database"]
                [nameof(DatabaseOptions.ConnectionStrings)]
                [nameof(ConnectionStrings.Game)]
                .ToValue().ToString();
            var dbPath = config;

            var options = new DbContextOptionsBuilder<GameContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            return new GameContext(options);
        }
    }
}
