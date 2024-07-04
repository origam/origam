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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Workbench.Services;
using System.Linq;

namespace Origam.OrigamEngine;
/// <summary>
/// Summary description for TranslationBuilder.
/// </summary>
public static class TranslationBuilder
{
    public static void Build(Stream stream, LocalizationCache currentTranslations, string locale, Guid packageId)
    {
        XmlTextWriter xtw = new XmlTextWriter(stream, System.Text.Encoding.UTF8);
        xtw.Formatting = Formatting.Indented;
        xtw.WriteStartDocument(true);
        xtw.WriteStartElement("OrigamLocalization");
        IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        IDocumentationService docSvc = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;
        List<AbstractSchemaItem> list = persistence
            .SchemaProvider
            .RetrieveListByPackage<AbstractSchemaItem>(packageId);
        foreach (AbstractSchemaItem item in list)
        {
            List<MemberAttributeInfo> memberList = Reflector.FindMembers(item.GetType(), typeof(LocalizableAttribute), new Type[] { });
            Hashtable values = new Hashtable();
            IQueryLocalizable ql = item as IQueryLocalizable;
            foreach (MemberAttributeInfo mai in memberList)
            {
                bool isLocalizable = true;
                if (ql != null)
                {
                    isLocalizable = ql.IsLocalizable(mai.MemberInfo.Name);
                }
                if (isLocalizable)
                {
                    object translatableValue = Reflector.GetValue(mai.MemberInfo, item);
                    if (translatableValue != null && translatableValue.ToString() != "")
                    {
                        values.Add(mai.MemberInfo.Name, translatableValue);
                    }
                }
            }
            DocumentationComplete docData = docSvc.LoadDocumentation(item.Id);
            if (values.Count > 0 || docData.Documentation.Count > 0)
            {
                xtw.WriteStartElement("Element");
                xtw.WriteAttributeString("Id", item.Id.ToString());
                xtw.WriteAttributeString("Path", item.Path);
                foreach (DictionaryEntry entry in values)
                {
                    xtw.WriteStartElement((string)entry.Key);
                    xtw.WriteAttributeString("OriginalText", entry.Value.ToString());
                    string translation;
                    if (currentTranslations != null)
                    {
                        translation = currentTranslations.GetLocalizedString(item.Id, (string)entry.Key, entry.Value.ToString(), locale);
                    }
                    else
                    {
                        translation = entry.Value.ToString();
                    }
                    xtw.WriteString(translation);
                    xtw.WriteEndElement();
                }
                string[] categoriesToInclude = GetDocumentationCategoriesToInclude();
                foreach (DocumentationComplete.DocumentationRow docRow in docData.Documentation)
                {
                    if (!categoriesToInclude.Contains(docRow.Category)) {
                        continue;
                    }
                    xtw.WriteStartElement("Documentation");
                    xtw.WriteAttributeString("Category", docRow.Category);
                    xtw.WriteAttributeString("OriginalText", docRow.Data.ToString());
                    string translation;
                    if (currentTranslations != null)
                    {
                        translation = currentTranslations.GetLocalizedString(item.Id, "Documentation " + docRow.Category, docRow.Data.ToString(), locale);
                    }
                    else
                    {
                        translation = docRow.Data.ToString();
                    }
                    xtw.WriteString(translation);
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
        if (settings.LocalizationIncludedDocumentationElements == null) { 
            return new string[0];
        }
        return settings.LocalizationIncludedDocumentationElements
            .Split(',')
            .Select(x => x.Trim())
            .Where(x => x != "")
            .ToArray();
    }
}
