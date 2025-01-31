using System.Data.SqlClient;

namespace E_Shop.Services
{
    internal class DbConnectionProvider
    {
        private static DbConnectionProvider? _instance = null;
        public static string? ConnectionString;

        public SqlConnection Connection;

        private DbConnectionProvider()
        {
            Connection = new SqlConnection(ConnectionString);
        }

        public static DbConnectionProvider GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DbConnectionProvider();
            }
            return _instance;
        }
    }
}
