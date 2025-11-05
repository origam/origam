using System;
using System.Linq;
using Origam.DA.Common.DatabasePlatform;

namespace Origam.DA.ObjectPersistence;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PostgresLengthLimitAttribute : AbstractModelElementRuleAttribute
{
    private readonly string errorMessageDetail;

    public PostgresLengthLimitAttribute(string errorMessageDetail = null)
    {
        this.errorMessageDetail = errorMessageDetail ?? "";
    }

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

        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        var databaseTypes = settings.GetAllPlatforms().Select(x => x.Name).ToList();

        if (!databaseTypes.Contains(nameof(DatabaseType.PgSql)))
        {
            return null;
        }

        string value = Reflector.GetValue(instance.GetType(), instance, memberName) as string;
        if (value == null)
        {
            return null;
        }

        if (value.Length > 63)
        {
            return new Exception(
                "Length limit exceeded. Max Postgre SQL entity name length is 63 characters. "
                    + errorMessageDetail
            );
        }
        return null;
    }
}
