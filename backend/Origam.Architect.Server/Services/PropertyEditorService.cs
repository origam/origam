#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Collections;
using System.Reflection;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Utils;
using Origam.DA;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;

namespace Origam.Architect.Server.Services;

public class PropertyEditorService(EditorPropertyFactory propertyFactory)
{
    private IEnumerable<EditorProperty> GetEditorPropertiesByName(ISchemaItem item, string[] names)
    {
        var properties = PropertyUtils
            .GetVisibleProperties(item.GetType())
            .Where(prop => names.Contains(prop.Name))
            .Select(prop => propertyFactory.Create(prop, item));
        return properties;
    }

    public List<string> GetRuleErrors(PropertyInfo property, ISchemaItem item)
    {
        List<string> ruleErrors = property
            .GetCustomAttributes()
            .OfType<IModelElementRule>()
            .Select(rule => rule.CheckRule(item, property.Name)?.Message)
            .Where(message => message != null)
            .ToList();
        return ruleErrors.Count == 0 ? null : ruleErrors;
    }

    public IEnumerable<EditorProperty> GetEditorProperties(ISchemaItem item)
    {
        if (item is XslTransformation xsltTransformation)
        {
            IEnumerable<EditorProperty> xsltProperties = GetEditorPropertiesByName(
                xsltTransformation,
                ["Id", "Package", "TextStore", "XsltEngineType", "Name"]
            );
            return xsltProperties;
        }

        if (item is XslRule xslRule)
        {
            IEnumerable<EditorProperty> xsltProperties = GetEditorPropertiesByName(
                xslRule,
                ["Xsl", "Name", "Structure"]
            );
            return xsltProperties;
        }

        IEnumerable<EditorProperty> properties = PropertyUtils
            .GetVisibleProperties(item.GetType())
            .Select(prop => propertyFactory.CreateIfMarkedAsEditable(prop, item))
            .Where(x => x != null);
        return properties;
    }

    public List<CollectionMember> GetCollectionMembers(ISchemaItem item, string propertyName)
    {
        PropertyInfo property = PropertyUtils
            .GetVisibleProperties(item.GetType())
            .First(property =>
                property.Name == propertyName
                && PropertyUtils.CanBeEdited(property)
                && PropertyUtils.IsCollection(property)
            );
        return propertyFactory.ToCollectionMembers((ICollection)property.GetValue(item));
    }

    public IEnumerable<EditorProperty> GetEditorPropertiesWithErrors(ISchemaItem item)
    {
        var editorProperties = GetEditorProperties(item)
            .Peek(property =>
            {
                property.Errors = GetRuleErrorsIfExist(property, item);
            });
        return editorProperties;
    }

    public List<string> GetRuleErrorsIfExist(EditorProperty editorProperty, ISchemaItem item)
    {
        PropertyInfo propertyInfo = PropertyUtils
            .GetVisibleProperties(item.GetType())
            .First(property => property.Name == editorProperty.Name);
        return GetRuleErrors(propertyInfo, item);
    }
}
