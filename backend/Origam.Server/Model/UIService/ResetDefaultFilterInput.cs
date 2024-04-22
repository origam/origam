using System;

namespace Origam.Server.Model.UIService;

public class ResetDefaultFilterInput
{
    public Guid SessionFormIdentifier { get; set; } = Guid.Empty;
    public Guid PanelInstanceId { get; set; }
}