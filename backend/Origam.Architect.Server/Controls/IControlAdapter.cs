using System.Reflection;

namespace Origam.Architect.Server.Controls;

public interface IControlAdapter
{
    PropertyInfo[] GetSchemaItemProperties();
    PropertyInfo[] GetValueItemProperties();
}