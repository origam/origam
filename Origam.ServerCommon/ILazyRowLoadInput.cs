using System;
using System.Collections.Generic;
using Origam.ServerCommon;

namespace Origam.Server
{
    public interface ILazyRowLoadInput: IEntityIdentification
    {
        Guid MenuId { get; set; }

        Guid DataStructureEntityId { get; set; }
        string Filter { get; set; }
        Dictionary<string, Guid> FilterLookups { get; set; }
        List<IRowOrdering> OrderingList { get; }

        int RowLimit { get; set; }        
        int RowOffset { get; set; }

        string[] ColumnNames { get; set; }
        Guid MasterRowId { get; set; }
        
        Guid SessionFormIdentifier { get; set; }
    }
}