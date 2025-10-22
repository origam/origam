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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.DA.Service.FileSystemModeCheckers;

class XmlReferencePropertyChecker : IFileSystemModelChecker
{
    private readonly FilePersistenceProvider filePersistenceProvider;

    public XmlReferencePropertyChecker(FilePersistenceProvider filePersistenceProvider)
    {
        this.filePersistenceProvider = filePersistenceProvider;
    }

    public IEnumerable<ModelErrorSection> GetErrors()
    {
        var allInstances = filePersistenceProvider.RetrieveList<IFilePersistent>().ToArray();
        var allTypes = allInstances.Select(x => x.GetType()).Distinct().ToArray();
        List<ErrorMessage> errors = allTypes
            .Select(GetTypeData)
            .SelectMany(typeData =>
            {
                return allInstances
                    .Where(instance => instance.GetType() == typeData.Type)
                    .SelectMany(instance =>
                        CheckReferencedObjectsExistAndReturnErrors(instance, typeData)
                    );
            })
            .ToList();
        yield return new ModelErrorSection(
            caption: "Invalid References Between Origam Files",
            errorMessages: errors
        );
    }

    private IEnumerable<ErrorMessage> CheckReferencedObjectsExistAndReturnErrors(
        IFilePersistent instance,
        TypeData typeData
    )
    {
        return typeData
            .XmlReferenceFieldInfos.Select(info =>
            {
                Guid refId = (Guid)info.GetValue(instance);
                if (refId == Guid.Empty)
                {
                    return null;
                }

                var referencedObject = filePersistenceProvider.RetrieveInstance<IFilePersistent>(
                    refId
                );
                if (referencedObject == null)
                {
                    return new ErrorMessage(
                        text: "Instance with id: "
                            + instance.Id
                            + " persisted in "
                            + GetFullPath(instance)
                            + " references "
                            + info.Name
                            + " with id: "
                            + refId
                            + ". The referenced object cannot be found.",
                        link: GetFullPath(instance)
                    );
                }
                return null;
            })
            .Where(errMessage => errMessage != null)
            .ToList();
    }

    private string GetFullPath(IFilePersistent instance)
    {
        return Path.Combine(
            filePersistenceProvider.TopDirectory.FullName,
            instance.RelativeFilePath
        );
    }

    private TypeData GetTypeData(Type type)
    {
        var fieldInfos = type.GetProperties()
            .Select(GetXmlRefAttributeOrNull)
            .Where(attr => attr != null)
            .Select(attr => attr.IdField)
            .Select(idFieldName => FindField(type, idFieldName))
            .ToArray();
        return new TypeData { Type = type, XmlReferenceFieldInfos = fieldInfos };
    }

    private MemberInfo FindField(Type type, string fieldName)
    {
        FieldInfo filedInfo = type.GetField(fieldName);
        if (filedInfo != null)
        {
            return filedInfo;
        }

        PropertyInfo propertyInfo = type.GetProperty(fieldName);
        if (propertyInfo != null)
        {
            return propertyInfo;
        }

        throw new Exception(
            "Type: " + type + " does not have property or field named: " + fieldName
        );
    }

    private XmlReferenceAttribute GetXmlRefAttributeOrNull(PropertyInfo prop)
    {
        return prop.GetCustomAttributes(true).OfType<XmlReferenceAttribute>().FirstOrDefault();
    }
}

class TypeData
{
    public Type Type { get; set; }
    public MemberInfo[] XmlReferenceFieldInfos { get; set; }
}
