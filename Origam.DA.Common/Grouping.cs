using System;

namespace Origam.DA
{
    public class Grouping
    {
        public string GroupBy { get; }
        public Guid LookupId { get; }

        public Grouping(string groupBy, Guid lookupId)
        {
            GroupBy = groupBy;
            LookupId = lookupId;
        }
    }
}