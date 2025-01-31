using DbToolkit.Enums;

namespace DbToolkit.Filtering
{
    public class Filter(string column, SqlOperator @operator, object value)
    {
        public string Column { get; set; } = column;
        public SqlOperator Operator { get; set; } = @operator;
        public object? Value { get; set; } = value;

        public string GetSqlOperator() => Operator switch
        {
            SqlOperator.Equal => "=",
            SqlOperator.GreaterThan => ">",
            SqlOperator.LessThan => "<",
            SqlOperator.GreaterThanOrEqual => ">=",
            SqlOperator.LessThanOrEqual => "<=",
            SqlOperator.NotEqual => "!=",
            SqlOperator.Like => "LIKE",
            _ => throw new NotImplementedException($"SQL operator '{Operator}' not supported.")
        };
    }
}
