using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace E_Shop.Services
{
    internal class DbAccess
    {
        private static DbAccess? _instance = null;

        private SqlConnection _connection;

        public static string? ConnectionString { get; set; }


        private DbAccess()
        {
            _connection = new SqlConnection(ConnectionString);
        }

        public static DbAccess GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DbAccess();
            }
            return _instance;
        }

        public DataTable Select(string table, string[] columns, string? condition = null, string? filter = null)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"SELECT {string.Join(",", columns)} FROM {table}");

            if (condition != null)
            {
                sb.Append($" WHERE {condition}");
            }
            if (filter != null)
            {
                sb.Append($" LIKE '{filter.Replace("'", "''")}'");
            }

            DataTable dataTable = new DataTable();

            using (SqlCommand command = new SqlCommand(sb.ToString(), _connection))
            {
                new SqlDataAdapter(command).Fill(dataTable);
            }

            return dataTable;
        }


        public void Insert(string table, string[] columns, string[] values)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"INSERT INTO {table} ({string.Join(",", columns)}) ");

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = $"'{values[i].Replace("'", "''")}'";
            }
            sb.Append($"VALUES ({string.Join(",", values)})");

            _connection.Open();

            using (SqlCommand command = new SqlCommand(sb.ToString(), _connection))
            {
                command.ExecuteNonQuery();
            }

            _connection.Close();
        }

        public void Update(string table, Dictionary<string, string> keyValues, string condition)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"UPDATE {table} SET ");
            sb.Append(string.Join(',', keyValues.Select(x => $"{x.Key} = '{x.Value.Replace("'", "''")}'").ToArray()));
            sb.Append($" WHERE {condition}");

            _connection.Open();

            using (SqlCommand command = new SqlCommand(sb.ToString(), _connection))
            {
                command.ExecuteNonQuery();
            }

            _connection.Close();

        }

        public void Delete (string table, string condition)
        {
            string cmd = $"DELETE FROM {table} WHERE {condition}";

            _connection.Open();

            using (SqlCommand command = new SqlCommand(cmd, _connection))
            {
                command.ExecuteNonQuery();
            }

            _connection.Close();
        }
    }
}
