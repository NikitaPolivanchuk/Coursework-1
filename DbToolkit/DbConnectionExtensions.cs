using System.Data;
using DbToolkit.Entities;
using QueryParameters = System.Collections.Generic.IDictionary<string, object?>;

namespace DbToolkit
{
    public static class DbConnectionExtensions
    {
        public static IList<T> Query<T>(this IDbConnection connection, string query, QueryParameters? parameters = null) where T : new()
        {
            var result = new List<T>();

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using var command = CreateCommand(connection, query, parameters);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var entity = EntityMapper.MapToEntity<T>(reader);
                result.Add(entity);
            }

            connection.Close();

            return result;
        }

        public static int Execute(this IDbConnection connection, string query, QueryParameters? parameters = null)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using var command = CreateCommand(connection, query, parameters);
            var result = command.ExecuteNonQuery();

            connection.Close();

            return result;
        }

        private static IDbCommand CreateCommand(IDbConnection connection, string query, QueryParameters? parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            return command;
        }

        private static void AddParameters(IDbCommand command, QueryParameters parameters)
        {
            foreach (var parameter in parameters)
            {
                var dbParameter = command.CreateParameter();

                dbParameter.ParameterName = parameter.Key;
                dbParameter.Value = parameter.Value ?? DBNull.Value;

                command.Parameters.Add(dbParameter);
            }
        }
    }
}
