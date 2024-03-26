using System.ComponentModel.DataAnnotations;

namespace Origam.DA
{
    public class Aggregation
    {
        [Required]
        public string ColumnName { get; }
        [Required]
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

    public class AggregationData : Aggregation
    {
        public object  Value { get; set; }
        
        public AggregationData(string columnName, CustomAggregationType aggregationType, object value) 
            : base(columnName, aggregationType)
        {
            this.Value = value;
        }
    }
}