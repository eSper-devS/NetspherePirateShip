using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;
using Netsphere.Database.Configuration;
using Hjson;
using Netsphere.Common.Configuration;

namespace Netsphere.Database
    {
        public class DesignTimeAuthContextFactory : IDesignTimeDbContextFactory<AuthContext>
        {
            public AuthContext CreateDbContext(string[] args)
            {
                var config = HjsonValue.Load("config.hjson")
                    ["Database"]
                    [nameof(DatabaseOptions.ConnectionStrings)]
                    [nameof(ConnectionStrings.Auth)]
                    .ToValue().ToString();



            var dbPath = config;

                var options = new DbContextOptionsBuilder<AuthContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;

                return new AuthContext(options);
            }
        }
    }
