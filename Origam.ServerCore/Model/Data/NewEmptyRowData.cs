using System;

namespace Origam.ServerCore.Model.Data
{
    public class NewEmptyRowData
    {
        [RequireNonDefault]
        public Guid DataStructureEntityId { get; set; }
        [RequireNonDefault]
        public Guid MenuId { get; set; }
    }
}