using Origam.Schema;
using System;

namespace Origam.Workbench.Pads
{
    public class RuleResult
    {
        public Key PrimaryKey { get; set; }
        public Type Type { get; set; }
        public Guid SchemaExtensionId { get; set; }

        public RuleResult(AbstractSchemaItem item)
        {
            this.PrimaryKey = item.PrimaryKey;
            this.Type = item.GetType();
            this.SchemaExtensionId = item.SchemaExtensionId;
        }
    }
}
