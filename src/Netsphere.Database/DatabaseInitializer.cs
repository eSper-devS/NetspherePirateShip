using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Netsphere.Database
{
    public static class DatabaseInitializer
    {
        public static void Initialize<TContext>(TContext context)
            where TContext : DbContext
        {
            var dbFile = context.Database.GetDbConnection().DataSource;

            var directory = Path.GetDirectoryName(dbFile);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            context.Database.Migrate();
        }
    }
}
