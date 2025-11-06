using System;
using Origam.DA.Common.DatabasePlatform;
using Origam.DA.ObjectPersistence;

namespace Origam.DA.Common.ObjectPersistence.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class IndexNameLengthLimitAttribute : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (string.IsNullOrEmpty(memberName))
        {
            CheckRule(instance);
        }

        var databaseProfile = DatabaseProfile.GetInstance();

        if (Reflector.GetValue(instance.GetType(), instance, memberName) is not string value)
        {
            return null;
        }

        string errorMessage = databaseProfile.CheckIndexNameLength(value.Length);
        if (!string.IsNullOrEmpty(errorMessage))
        {
            return new Exception(errorMessage);
        }
        return null;
    }
}
