namespace Origam.Architect.Server.Controllers;

public class MenuItemInfo
{
    public string Caption { get; init; }
    public string TypeName { get; init; }
    public string IconName { get; init; }
    public int? IconIndex { get; init; }

    public MenuItemInfo(string caption, string typeName, string iconName, int? iconIndex)
    {
        Caption = caption;
        TypeName = typeName;
        IconName = iconName;
        IconIndex = iconIndex;
    }
}