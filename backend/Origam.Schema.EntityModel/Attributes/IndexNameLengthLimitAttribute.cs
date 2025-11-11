using System;
using System.Linq;
using Origam.DA.ObjectPersistence;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class IndexNameLengthLimitAttribute : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(Strings.MemberNameRequired);
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (instance is not TableMappingItem table)
        {
            throw new Exception(
                nameof(IndexNameLengthLimitAttribute) + " can only be applied to TableMappingItem"
            );
        }

        if (string.IsNullOrEmpty(memberName))
        {
            CheckRule(instance);
        }

        var databaseProfile = ServiceManager.Services.GetService<DatabaseProfileService>();

        var indices = table
            .Ancestors.Cast<SchemaItemAncestor>()
            .SelectMany(x =>
                x.SchemaItem.ChildItemsByType<DataEntityIndex>(DataEntityIndex.CategoryConst)
            );

        string errorMessage = "";
        foreach (DataEntityIndex entityIndex in indices)
        {
            string finalIndexName = entityIndex.MakeDatabaseName(table);
            errorMessage += databaseProfile.CheckIndexNameLength(finalIndexName);
        }
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            return new Exception(errorMessage);
        }
        return null;
    }
}
