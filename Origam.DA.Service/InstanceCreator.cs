#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.DA.Service
{
    abstract class AbstractInstanceCreator
    {
        private readonly ParentFolders parentFolderIds;

        protected AbstractInstanceCreator(ParentFolders parentFolderIds)
        {
            this.parentFolderIds = parentFolderIds;
        }

        public IFilePersistent RetrieveInstance( Guid id,
            IPersistenceProvider provider, Guid parentId)
        {
            IFilePersistent instance = Instantiate(id, provider, parentId);
  
            SetXmlAttributes( instance, provider);
            NoteExternalReferences(instance);
            SetParentAttributes(parentId, instance, provider);
            SetReferences(instance);
            instance.UseObjectCache = false;
            return instance;
        }
        protected abstract void SetValue(IFilePersistent instance, MemberAttributeInfo mi, object value, IPersistenceProvider provider);

        protected abstract void SetXmlAttributes(IFilePersistent instance, IPersistenceProvider provider);

        protected abstract void NoteExternalReferences(IFilePersistent instance);

        protected abstract void SetReferences(IFilePersistent instance);

        protected abstract string GetTypeName();

        private void SetParentAttributes(Guid parentId, IFilePersistent instance,
            IPersistenceProvider provider)
        {
            IList parentFolderReferences = Reflector.FindMembers(
                instance.GetType(), typeof(XmlParentAttribute));
            bool isTopFileElement = parentId == Guid.Empty;
            foreach (MemberAttributeInfo mi in parentFolderReferences)
            {
                XmlParentAttribute attribute = mi.Attribute as XmlParentAttribute;
                ElementName folderUri = ElementNameFactory.Create(attribute.Type);
                if (folderUri == OrigamFile.GroupNameUri && !isTopFileElement) continue;
                if (parentFolderIds.ContainsKey(folderUri))
                {
                    SetValue(instance, mi, parentFolderIds[folderUri], provider);
                }
            }
        }

        private IFilePersistent Instantiate( Guid id,
            IPersistenceProvider provider, Guid parentId)
        {
            if(provider == null) throw new ArgumentNullException("provider is null");
            string typeName = GetTypeName();
            string assemblyName = typeName.Substring(0, typeName.LastIndexOf('.'));
            Key key = new Key();
            key.Add("Id", id);
            object[] constructorArray = {key};
            IFilePersistent instance = Reflector.InvokeObject(assemblyName, typeName, constructorArray)
                as IFilePersistent;
            // Set the persistence provider, so the object can persist itself later on
            instance.PersistenceProvider = provider;
            // Mark the object as persisted, because we have just read it from the data source
            instance.IsPersisted = true;
            // set a parent right now because setting other properties might use it
            instance.FileParentId = parentId;
            return instance;
        }
    }
    
    class InstanceCloner : AbstractInstanceCreator
    {

        private readonly object source;

        public InstanceCloner(object source, ParentFolders parentFolders)
            : base(parentFolders)
        {
            this.source = source;
        }

        protected override void SetValue(IFilePersistent instance, MemberAttributeInfo mi, 
            object value, IPersistenceProvider provider)
        {
            Reflector.SetValue(mi.MemberInfo, instance, value);
        }
        
        protected override void SetXmlAttributes(IFilePersistent instance, IPersistenceProvider provider)
        {
            IList members = Reflector.FindMembers(
                instance.GetType(), typeof(XmlAttributeAttribute));
            foreach (MemberAttributeInfo mi in members)
            {
                object value = mi.MemberInfo.GetValue(source);
                SetValue(instance, mi, value, provider);
            }
        }

        protected override void NoteExternalReferences(IFilePersistent instance)
        {
            // was done when InstanceCreator created the source, no need to repeat it here
        }

        protected override void SetReferences(IFilePersistent instance)
        {
            IList references = Reflector.FindMembers(
                instance.GetType(), typeof(XmlReferenceAttribute));
            foreach (MemberAttributeInfo mi in references)
            {
                XmlReferenceAttribute attribute = mi.Attribute as XmlReferenceAttribute;

                string attributeIdField = attribute.IdField;
                Guid refId = (Guid)Reflector
                    .GetValue(source.GetType(), source, attributeIdField);

                Reflector.SetValue(instance, attribute.IdField,refId);
            }
        }

        protected override string GetTypeName() 
            => source.GetType().ToString();
    }
    
    class InstanceCreator: AbstractInstanceCreator
    {
        private readonly ExternalFileManager externalFileManger;
        private readonly XmlReader reader;

        public InstanceCreator( XmlReader reader, ParentFolders parentFolderIds,
            ExternalFileManager externalFileManger) : base(parentFolderIds)
        {
            this.externalFileManger = externalFileManger;
            this.reader = reader;
        }
        
        protected override void SetReferences(IFilePersistent instance)
        {
            IList references = Reflector.FindMembers(
                instance.GetType(), typeof(XmlReferenceAttribute));
            foreach (MemberAttributeInfo mi in references)
            {
                XmlReferenceAttribute attribute = mi.Attribute as XmlReferenceAttribute;
                string value =
                    reader.GetAttribute(attribute.AttributeName, attribute.Namespace);
                // reference looks like "/package/folder/entity.xml/entity/field#GUID
                // we only read the guid part
                if (value != null)
                {
                    Reflector.SetValue(instance, attribute.IdField,
                        new Guid(value.Substring(value.Length - 36)));
                }
            }
        }

        protected override string GetTypeName() =>  XmlUtils.ReadType(reader);

        protected override void NoteExternalReferences(IFilePersistent instance)
        {
            IList externalFileReferences = Reflector.FindMembers(
                instance.GetType(), typeof(XmlExternalFileReference));
            foreach (MemberAttributeInfo mi in externalFileReferences)
            {
                var attribute = (XmlExternalFileReference) mi.Attribute;
                string externalLink =
                    reader.GetAttribute(attribute.ContainerName, attribute.Namespace);
                externalFileManger.AddFileLink(externalLink);
            }
        }

        protected override void SetXmlAttributes( IFilePersistent instance,
            IPersistenceProvider provider)
        {
            IList members = Reflector.FindMembers(
                instance.GetType(), typeof(XmlAttributeAttribute));
            foreach (MemberAttributeInfo mi in members)
            {
                XmlAttributeAttribute attribute = mi.Attribute as XmlAttributeAttribute;
                string value =
                    reader.GetAttribute(attribute.AttributeName, attribute.Namespace);
                SetValue(instance, mi, value, provider);
            }
        }

        protected override void SetValue(IFilePersistent instance, MemberAttributeInfo mi,
            object value, IPersistenceProvider provider)
        {
            Type memberType;
            if (mi.MemberInfo is PropertyInfo)
                memberType = (mi.MemberInfo as PropertyInfo).PropertyType;
            else
                memberType = (mi.MemberInfo as FieldInfo).FieldType;

            object correctlyTypedValue;
            // If member is enum, we have to convert
            if (memberType.IsEnum )
            {
                correctlyTypedValue = value == null ? null :
                    Enum.Parse(memberType, (string)value);
            }
            else if (memberType == typeof(int))
            {
                correctlyTypedValue = value == null ? 0 :
                    XmlConvert.ToInt32((string)value);
            }
            else if (memberType == typeof(bool))
            {
                correctlyTypedValue = value != null &&
                                      XmlConvert.ToBoolean((string)value);
            }
            else if (memberType == typeof(Guid))
            {
                if (value != null)
                {
                    correctlyTypedValue 
                        = value is Guid ? value : new Guid((string)value);
                }
                else
                {
                    correctlyTypedValue = null;
                }
            }
            else
            {
                correctlyTypedValue = value;
            }
            if ( correctlyTypedValue is string strValue)
            {
                correctlyTypedValue = provider.LocalizationCache
                    .GetLocalizedString(instance.Id, mi.MemberInfo.Name, strValue);
            }
            Reflector.SetValue(mi.MemberInfo, instance, correctlyTypedValue);
        }       
    }
}