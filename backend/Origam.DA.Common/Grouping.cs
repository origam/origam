using System;

namespace Origam.DA;
public class Grouping
{
    public string GroupBy { get; }
    public Guid LookupId { get; }
    public string GroupingUnit { get; }
    public Grouping(string groupBy, Guid lookupId, string groupingUnit)
    {
        GroupBy = groupBy;
        LookupId = lookupId;
        GroupingUnit = groupingUnit;
    }
}
