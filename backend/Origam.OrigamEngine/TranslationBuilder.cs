#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine;

/// <summary>
/// Summary description for TranslationBuilder.
/// </summary>
public static class TranslationBuilder
{
    public static void Build(
        Stream stream,
        LocalizationCache currentTranslations,
        string locale,
        Guid packageId
    )
    {
        XmlTextWriter xtw = new XmlTextWriter(w: stream, encoding: System.Text.Encoding.UTF8);
        xtw.Formatting = Formatting.Indented;
        xtw.WriteStartDocument(standalone: true);
        xtw.WriteStartElement(localName: "OrigamLocalization");
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        IDocumentationService docSvc =
            ServiceManager.Services.GetService(serviceType: typeof(IDocumentationService))
            as IDocumentationService;
        List<ISchemaItem> list = persistence.SchemaProvider.RetrieveListByPackage<ISchemaItem>(
            packageId: packageId
        );
        foreach (ISchemaItem item in list)
        {
            List<MemberAttributeInfo> memberList = Reflector.FindMembers(
                type: item.GetType(),
                primaryAttribute: typeof(LocalizableAttribute),
                secondaryAttributes: new Type[] { }
            );
            Hashtable values = new Hashtable();
            IQueryLocalizable ql = item as IQueryLocalizable;
            foreach (MemberAttributeInfo mai in memberList)
            {
                bool isLocalizable = true;
                if (ql != null)
                {
                    isLocalizable = ql.IsLocalizable(member: mai.MemberInfo.Name);
                }
                if (isLocalizable)
                {
                    object translatableValue = Reflector.GetValue(
                        memberInfo: mai.MemberInfo,
                        instance: item
                    );
                    if (translatableValue != null && translatableValue.ToString() != "")
                    {
                        values.Add(key: mai.MemberInfo.Name, value: translatableValue);
                    }
                }
            }
            DocumentationComplete docData = docSvc.LoadDocumentation(schemaItemId: item.Id);
            if (values.Count > 0 || docData.Documentation.Count > 0)
            {
                xtw.WriteStartElement(localName: "Element");
                xtw.WriteAttributeString(localName: "Id", value: item.Id.ToString());
                xtw.WriteAttributeString(localName: "Path", value: item.Path);
                foreach (DictionaryEntry entry in values)
                {
                    xtw.WriteStartElement(localName: (string)entry.Key);
                    xtw.WriteAttributeString(
                        localName: "OriginalText",
                        value: entry.Value.ToString()
                    );
                    string translation;
                    if (currentTranslations != null)
                    {
                        translation = currentTranslations.GetLocalizedString(
                            elementId: item.Id,
                            memberName: (string)entry.Key,
                            defaultString: entry.Value.ToString(),
                            locale: locale
                        );
                    }
                    else
                    {
                        translation = entry.Value.ToString();
                    }
                    xtw.WriteString(text: translation);
                    xtw.WriteEndElement();
                }
                string[] categoriesToInclude = GetDocumentationCategoriesToInclude();
                foreach (DocumentationComplete.DocumentationRow docRow in docData.Documentation)
                {
                    if (!categoriesToInclude.Contains(value: docRow.Category))
                    {
                        continue;
                    }
                    xtw.WriteStartElement(localName: "Documentation");
                    xtw.WriteAttributeString(localName: "Category", value: docRow.Category);
                    xtw.WriteAttributeString(
                        localName: "OriginalText",
                        value: docRow.Data.ToString()
                    );
                    string translation;
                    if (currentTranslations != null)
                    {
                        translation = currentTranslations.GetLocalizedString(
                            elementId: item.Id,
                            memberName: "Documentation " + docRow.Category,
                            defaultString: docRow.Data.ToString(),
                            locale: locale
                        );
                    }
                    else
                    {
                        translation = docRow.Data.ToString();
                    }
                    xtw.WriteString(text: translation);
                    xtw.WriteEndElement();
                }
                xtw.WriteEndElement();
            }
        }
        xtw.WriteEndElement();
        xtw.WriteEndDocument();
        xtw.Flush();
    }

    private static string[] GetDocumentationCategoriesToInclude()
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        if (settings.LocalizationIncludedDocumentationElements == null)
        {
            return new string[0];
        }
        return settings
            .LocalizationIncludedDocumentationElements.Split(separator: ',')
            .Select(selector: x => x.Trim())
            .Where(predicate: x => x != "")
            .ToArray();
    }
}
