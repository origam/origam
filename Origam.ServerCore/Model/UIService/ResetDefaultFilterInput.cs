using System;

namespace Origam.ServerCore.Model.UIService
{
    public class ResetDefaultFilterInput
    {
        public Guid SessionFormIdentifier { get; set; } = Guid.Empty;
        public Guid PanelInstanceId { get; set; }
    }
}