using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace DbToolkit.Entities
{
    public class EntityMetadataProvider
    {
        private readonly Type type;

        public EntityMetadataProvider(Type type)
        {
            this.type = type;
        }

        public string GetTableName()
        {
            var tableAttribute = type.GetCustomAttribute<TableAttribute>()
                ?? throw new InvalidOperationException($"The class {type.Name} does not have a {nameof(TableAttribute)}.");

            return tableAttribute.Name;
        }

        public (string, object?) GetPrimaryKeyColumn<T>(T entity)
        {
            var keyProperty = type.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null)
                ?? throw new InvalidOperationException($"The class {type.Name} does not have a property with the {nameof(KeyAttribute)}.");

            var columnAttribute = keyProperty.GetCustomAttribute<ColumnAttribute>()
                ?? throw new InvalidOperationException($"Property {keyProperty.Name} does not have {nameof(KeyAttribute)}");

            return (columnAttribute.Name!, keyProperty.GetValue(entity));
        }

        public IDictionary<string, object?> GetColumns<T>(T entity)
        {
            var properties = entity!.GetType().GetProperties();
            var columns = new Dictionary<string, object?>();

            foreach (var property in properties)
            {
                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null && !string.IsNullOrEmpty(columnAttribute.Name))
                {
                    columns.Add(columnAttribute.Name!, property.GetValue(entity));
                }
            }

            if (columns.Count < 1)
            {
                throw new InvalidOperationException($"The class {entity.GetType().Name} does not have any properties with {nameof(ColumnAttribute)}.");
            }

            return columns;
        }

        public IReadOnlyList<string> GetColumnNames()
        {
            var properties = type.GetProperties();
            var columns = new List<string>();

            foreach (var property in properties)
            {
                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null && !string.IsNullOrEmpty(columnAttribute.Name))
                {
                    columns.Add(columnAttribute.Name!);
                }
            }

            if (columns.Count < 1)
            {
                throw new InvalidOperationException($"The class {type.Name} does not have any properties with {nameof(ColumnAttribute)}.");
            }

            return columns;
        }
    }
}
