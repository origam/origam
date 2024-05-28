using System;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

[AttributeUsage(validOn:AttributeTargets.Property)]
public class NoRuleForExportRowLevelSecurityRule 
    : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        if (instance is not EntitySecurityRule entitySecurityRule)
        {
            return new Exception(
                $"{nameof(NoRuleForExportRowLevelSecurityRule)} can be only applied to type {nameof(EntitySecurityRule)}");  
        }
        if (entitySecurityRule.ExportCredential
            && entitySecurityRule.Rule is not null)
        {
            return new Exception(
                "Export row level security rule can't have a rule set.");
        }
        return null;
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        return CheckRule(instance);
    }
}