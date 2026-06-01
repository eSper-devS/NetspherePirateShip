using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Netsphere.Database.Configuration
{
    public class DatabasesConfig
    {
        [JsonProperty("engine")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DatabaseEngine Engine { get; set; }

        [JsonProperty("auth")]
        public DatabaseConfig Auth { get; set; }

        [JsonProperty("game")]
        public DatabaseConfig Game { get; set; }

        public DatabasesConfig()
        {
            Engine = DatabaseEngine.SQLite;

            Auth = new DatabaseConfig
            {
                Filename = "..\\db\\auth.db"
            };

            Game = new DatabaseConfig
            {
                Filename = "..\\db\\game.db"
            };
        }

        public class DatabaseConfig
        {
            [JsonProperty("filename")]
            public string Filename { get; set; }

            [JsonProperty("host")]
            public string Host { get; set; }

            [JsonProperty("port")]
            public int Port { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("password")]
            public string Password { get; set; }

            [JsonProperty("database")]
            public string Database { get; set; }

            public DatabaseConfig()
            {
                Host = "localhost";
                Port = 3306;
            }
        }
    }

    public enum DatabaseEngine
    {
        SQLite,
        MySQL
    }
}
