using System;

namespace Origam.ServerCore.Model.UIService
{
    public class NewEmptyRowInput
    {
        [RequireNonDefault]
        public Guid DataStructureEntityId { get; set; }
        [RequireNonDefault]
        public Guid MenuId { get; set; }
    }
}