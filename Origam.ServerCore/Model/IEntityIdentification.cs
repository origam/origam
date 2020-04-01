using System;

namespace Origam.ServerCore.Model
{
    public interface IEntityIdentification
    {
        Guid DataStructureEntityId { get; set; }
        Guid MenuId { get; set; }
        Guid SessionFormIdentifier { get; set; }
    }
}