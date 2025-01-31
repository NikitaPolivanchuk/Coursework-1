using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;

namespace DbToolkit.Entities
{
    public static class EntityMapper
    {
        internal static T MapToEntity<T>(IDataReader reader) where T : new()
        {
            var entity = new T();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                var columnAttribute = prop.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null && !string.IsNullOrEmpty(columnAttribute.Name))
                {
                    var value = reader[columnAttribute.Name!];

                    if (value != DBNull.Value)
                    {
                        prop.SetValue(entity, Convert.ChangeType(value, prop.PropertyType));
                    }
                }
            }

            return entity;
        }
    }
}
