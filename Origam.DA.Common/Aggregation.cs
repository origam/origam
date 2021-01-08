namespace Origam.DA
{
    public class Aggregation
    {
        public string ColumnName { get; }
        public CustomAggregationType AggregationType { get; }
        public string SqlQueryColumnName => ColumnName + AggregationType;

        public Aggregation(string columnName, CustomAggregationType aggregationType)
        {
            ColumnName = columnName;
            AggregationType = aggregationType;
        }
    }
    
    public enum CustomAggregationType
    {
        Sum, Avg, Min, Max, Count
    }
}