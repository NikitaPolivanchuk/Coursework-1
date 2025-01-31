using System.Data;
using System.Text;
using DbToolkit.Entities;
using DbToolkit.Enums;
using DbToolkit.Filtering;

namespace DbToolkit
{
    public static class DbOperations
    {
        public static IList<T> Select<T>(this IDbConnection connection, Filters? filters = null) where T : new()
        {
            var type = typeof(T);

            var metadataExtractor = new EntityMetadataProvider(type);

            var table = metadataExtractor.GetTableName();
            var columns = metadataExtractor.GetColumnNames();

            var query = new StringBuilder();
            var parameters = new Dictionary<string, object?>();

            query.AppendLine($"SELECT {string.Join(',', columns)}");
            query.AppendLine($"FROM {table}");

            AddFilters(query, parameters, filters);

            return connection.Query<T>(query.ToString(), parameters);
        }

        public static bool Insert<T>(this IDbConnection connection, T entity) where T : new()
        {
            var type = typeof(T);

            var metadataExtractor = new EntityMetadataProvider(type);

            var table = metadataExtractor.GetTableName();
            var parameters = metadataExtractor.GetColumns(entity);
            var (primaryKeyColumn, primaryKeyValue) = metadataExtractor.GetPrimaryKeyColumn(entity);

            parameters.Remove(primaryKeyColumn);

            var columns = parameters.Select(item => item.Key).ToList();
            var values = parameters.Select(item => $"@{item.Key}").ToList();

            var query = new StringBuilder();
            query.AppendLine($"INSERT INTO {table} ({string.Join(',', columns)})");
            query.AppendLine($"VALUES ({string.Join(',', values)})");

            var affectedRows = connection.Execute(query.ToString(), parameters);
            return affectedRows > 0;
        }

        public static bool Update<T>(this IDbConnection connection, T entity) where T : new()
        {
            var type = typeof(T);

            var metadataExtractor = new EntityMetadataProvider(type);

            var table = metadataExtractor.GetTableName();
            var parameters = metadataExtractor.GetColumns(entity);
            var (primaryKeyColumn, primaryKeyValue) = metadataExtractor.GetPrimaryKeyColumn(entity);

            if (primaryKeyValue == null)
            {
                return false;
            }

            parameters.Remove(primaryKeyColumn);
            var setClauses = parameters.Select(item => $"{item.Key} = @{item.Key}");

            var query = new StringBuilder();
            query.AppendLine($"UPDATE {table} SET");
            query.AppendLine(string.Join($",{Environment.NewLine}", setClauses));

            var filters = new Filters();
            filters.AddFilter(primaryKeyColumn, SqlOperator.Equal, primaryKeyValue);
            AddFilters(query, parameters, filters);            

            var affectedColumns = connection.Execute(query.ToString(), parameters);
            return affectedColumns > 0;
        }

        public static bool Delete<T>(this IDbConnection connection, T entity) where T : new()
        {
            var type = typeof(T);

            var metadataExtractor = new EntityMetadataProvider(type);

            var table = metadataExtractor.GetTableName();
            var (primaryKeyColumn, primaryKeyValue) = metadataExtractor.GetPrimaryKeyColumn(entity);

            if (primaryKeyValue == null)
            {
                return false;
            }

            var query = new StringBuilder();
            var parameters = new Dictionary<string, object?>();

            query.AppendLine($"DELETE FROM {table}");

            var filters = new Filters();
            filters.AddFilter(primaryKeyColumn, SqlOperator.Equal, primaryKeyValue);
            AddFilters(query, parameters, filters);

            var affectedRows = connection.Execute(query.ToString(), parameters);
            return affectedRows > 0;
        }

        private static void AddFilters(StringBuilder query, IDictionary<string, object?> parameters, Filters? filters)
        {
            var sqlFilters = filters?.ToSql();
            if (!string.IsNullOrEmpty(sqlFilters))
            {
                query.AppendLine($"WHERE {sqlFilters}");
            }
            
            var extraParameters = filters?.ToDictionary();
            if (extraParameters != null)
            {
                foreach (var parameter in extraParameters)
                {
                    parameters.Add(parameter.Key, parameter.Value);
                }
            }
        }
    }
}
