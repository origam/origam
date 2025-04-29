using System;
using System.Collections.Generic;

namespace Origam.Server;
public class UpdateData
{
    public Guid RowId { get; set; }
    public Dictionary<string, object> Values { get; set; }
}
