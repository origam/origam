using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.DA.Service.FileSystemModeCheckers
{
    class XmlReferencePropertyChecker: IFileSystemModelChecker
    {
        private readonly FilePersistenceProvider filePersistenceProvider;

        public XmlReferencePropertyChecker(FilePersistenceProvider filePersistenceProvider)
        {
            this.filePersistenceProvider = filePersistenceProvider;
        }

        public List<string> GetErrors()
        {
            var allInstances = filePersistenceProvider
                .RetrieveList<IFilePersistent>()
                .ToArray();

            var allTypes = allInstances
                .Select(x => x.GetType())
                .Distinct()
                .ToArray();

            return
                allTypes
                    .Select(GetTypeData)
                    .SelectMany(typeData =>
                    {
                        return allInstances
                            .Where(instance => instance.GetType() == typeData.Type)
                            .SelectMany(instance => CheckReferencedObjectsExistAndReturnErrors(instance, typeData));
                    })
                    .ToList();
        }

        private IEnumerable<string> CheckReferencedObjectsExistAndReturnErrors(IFilePersistent instance, TypeData typeData)
        {
            return typeData
                .XmlReferenceFieldInfos
                .Select(info =>
                {
                    Guid refId = (Guid) info.GetValue(instance);
                    if (refId == Guid.Empty) return null;

                    var referencedObject = filePersistenceProvider.RetrieveInstance<IFilePersistent>(refId);
                    if (referencedObject == null)
                    {
                        return "Instance with id:" + instance.Id + " persisted in " +
                               instance.RelativeFilePath +
                               " references " + info.Name + " with id: " + refId +
                               ". The referenced object cannot be found.";
                    }

                    return null;
                })
                .Where(errMessage => errMessage != null)
                .ToList();
        }

        private TypeData GetTypeData(Type type)
        {
            var fieldInfos = type
                .GetProperties()
                .Select(GetXmlRefAttributeOrNull)
                .Where(attr => attr != null)
                .Select(attr => attr.IdField)
                .Select(idFieldName => FindField(type, idFieldName))
                .ToArray();

            return 
                new TypeData
                {
                    Type = type,
                    XmlReferenceFieldInfos = fieldInfos
                };
        }

        private MemberInfo FindField(Type type, string fieldName)
        {
            FieldInfo filedInfo = type.GetField(fieldName);
            if (filedInfo != null) return filedInfo;

            PropertyInfo propertyInfo = type.GetProperty(fieldName);
            if (propertyInfo != null) return propertyInfo;

            throw new Exception("Type: "+type+" does not have property or field named: "+fieldName);
        }

        private XmlReferenceAttribute GetXmlRefAttributeOrNull(PropertyInfo prop)
        {
            return prop.GetCustomAttributes(true)
                .OfType<XmlReferenceAttribute>()
                .FirstOrDefault();
        }
    }

    class TypeData
    {
        public Type Type { get; set; }
        public MemberInfo[] XmlReferenceFieldInfos { get; set; }
    }
}
