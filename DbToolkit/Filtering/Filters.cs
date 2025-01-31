using DbToolkit.Enums;

namespace DbToolkit.Filtering
{
    public class Filters(LogicalOperator logicalOperator = LogicalOperator.And)
    {
        public List<Filter> Conditions { get; set; } = [];

        public List<Filters> NestedFilters { get; set; } = [];

        public LogicalOperator LogicalOperator { get; set; } = logicalOperator;

        public void AddFilter(string column, SqlOperator @operator, object value)
        {
            Conditions.Add(new Filter(column, @operator, value));
        }

        public void AddNestedFilters(Filters filters)
        {
            NestedFilters.Add(filters);
        }

        public void SetLogicalOperator(LogicalOperator logicalOperator)
        {
            LogicalOperator = logicalOperator;
        }

        public string ToSql()
        {
            if (Conditions.Count == 0 && NestedFilters.Count == 0)
            {
                return string.Empty;
            }

            var clauses = new List<string>();

            if (Conditions.Count > 0)
            {
                clauses.AddRange(Conditions.Select(f => $"{f.Column} {f.GetSqlOperator()} @{f.Column}"));
            }

            if (NestedFilters.Count > 0)
            {
                foreach (var nested in NestedFilters)
                {
                    var nestedClause = nested.ToSql();
                    if (!string.IsNullOrEmpty(nestedClause))
                    {
                        clauses.Add($"({nestedClause})");
                    }
                }
            }

            var logicalOperatorString = LogicalOperator == LogicalOperator.And ? "AND" : "OR";

            return string.Join($" {logicalOperatorString} ", clauses);
        }

        public Dictionary<string, object?> ToDictionary() 
            => Conditions.ToDictionary(f => f.Column, f => f.Value);
    }
}
