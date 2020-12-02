using System;

namespace Origam.Server
{
    public interface IRowOrdering
    {
        string ColumnId { get; set; }

        string Direction { get; set; }

        Guid LookupId { get; set; }
    }
}