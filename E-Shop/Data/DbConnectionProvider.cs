using System.Data;
using System.Data.SqlClient;
using Webserver.Services;

namespace E_Shop.Data;

public class DbConnectionProvider
{
    private readonly string connectionString;

    public IDbConnection Connection => new SqlConnection(connectionString);

    public DbConnectionProvider(IConfigurationProvider configProvider)
    {
        connectionString = configProvider.GetSetting("ConnectionString")
            ?? throw new ArgumentException("Provide valid 'ConnectionString' configuration");

        using var connection = new SqlConnection(connectionString);
        connection.Open();
        connection.Close();
    }
}
