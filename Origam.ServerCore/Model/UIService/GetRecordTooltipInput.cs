using System;

namespace Origam.ServerCore.Model.UIService
{
    public class GetRecordTooltipInput
    {
        [RequiredNonDefault]
        public Guid DataStructureEntityId { get; set; }
        [RequiredNonDefault]
        public Guid RowId { get; set; }
        [RequiredNonDefault]
        public Guid MenuId { get; set; }
    }
}