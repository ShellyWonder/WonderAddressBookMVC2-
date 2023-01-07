using Microsoft.EntityFrameworkCore;
using Npgsql;
using WonderAddressBookMVC_.Data;

namespace WonderAddressBookMVC_.Services
{
    public class ConnectionService
    {
        public static string? GetConnectionString(IConfiguration configuration)
        {
            //Local environment
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            //Identifies the deployment environment
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }
        private static string BuildConnectionString(string databaseUrl)
        {
            //postgres specific
            //converts URL to URI; URL locates a resource, URI identifies a resource
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,//may need to change when deploying to RailWay
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        

    }
}
