using Origam.DA;
using Origam.Security.Common;
using System;
using static Origam.DA.Common.Enums;

namespace Origam.ProjectAutomation.Builders;
public class NewUserBuilder : AbstractDatabaseBuilder
{
    DatabaseType _databaseType;
    public override string Name => "Create Web User";
    public override void Execute(Project project)
    {
        var adaptivePassword = new InternalPasswordHasherWithLegacySupport();
        string hashPassword = adaptivePassword.HashPassword(project.WebUserPassword);
        _databaseType = project.DatabaseType;
        DataService(_databaseType).DbUser = project.Name;
        DataService(_databaseType).ConnectionString = project.BuilderDataConnectionString;
        QueryParameterCollection parameters = new QueryParameterCollection();
        parameters.Add(new QueryParameter("Id", Guid.NewGuid().ToString()));
        parameters.Add(new QueryParameter("UserName", project.WebUserName));
        parameters.Add(new QueryParameter("Password", hashPassword));
        parameters.Add(new QueryParameter("FirstName", project.WebFirstName));
        parameters.Add(new QueryParameter("Name", project.WebSurname));
        parameters.Add(new QueryParameter("Email", project.WebEmail));
        parameters.Add(new QueryParameter("RoleId", "E0AD1A0B-3E05-4B97-BE38-12FF63E7F2F2"));
        parameters.Add(new QueryParameter("RequestEmailConfirmation",
            "false"));
        DataService(_databaseType).CreateFirstNewWebUser(parameters);
    }
    private string BuildConnectionString(Project project)
    {
        return DataService(_databaseType).BuildConnectionString(project.DatabaseServerName, project.Port,
        project.DataDatabaseName, project.DatabaseUserName,
        project.DatabasePassword, project.DatabaseIntegratedAuthentication, false);
    }
    public override void Rollback()
    {
        
    }
}
