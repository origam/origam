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

using System.Collections.Generic;
using System.Xml;

namespace Origam.ServiceCore
{
    public class XmlReaderCore : XmlReader
    {
        private Dictionary<int, int> attributeMappingDictionary;
        private readonly XmlReader innerReader;
        private CachedElement cachedElement;
        public XmlReaderCore(XmlReader reader)
        {
            innerReader = reader;
        }
        public override int AttributeCount
        {
            get
            {
                if (cachedElement != null)
                {
                    return 0;
                }

                attributeMappingDictionary = new Dictionary<int, int>();
                int realPosition = 0;
                int fakePosition = 0;
                while (MoveToNextAttribute())
                {
                    if (!string.IsNullOrEmpty(Value))
                    {
                        attributeMappingDictionary.Add(fakePosition, realPosition);
                        fakePosition++;
                    }
                    realPosition++;
                }
                return attributeMappingDictionary.Count;
            }
        }

        public override string BaseURI => innerReader.BaseURI;

        public override int Depth => 
            cachedElement?.Depth ?? innerReader.Depth;

        public override bool EOF => innerReader.EOF;

        public override bool IsEmptyElement => innerReader.IsEmptyElement;

        public override string LocalName => 
            cachedElement?.LocalName ?? innerReader.LocalName;

        public override string NamespaceURI => 
            cachedElement?.NamespaceURI ?? innerReader.NamespaceURI;

        public override XmlNameTable NameTable => innerReader.NameTable;

        public override XmlNodeType NodeType => 
            cachedElement?.NodeType ?? innerReader.NodeType;

        public override string Prefix => 
            cachedElement?.Prefix ?? innerReader.Prefix;

        public override ReadState ReadState => innerReader.ReadState;

        public override string Value => 
            cachedElement?.Value ?? innerReader.Value;

        public override string GetAttribute(int i)
        {
            return innerReader.GetAttribute(attributeMappingDictionary[i]);
        }

        public override string GetAttribute(string name)
        {
            return innerReader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return innerReader.GetAttribute(name,namespaceURI);
        }

        public override string LookupNamespace(string prefix)
        {
            return innerReader.LookupNamespace(prefix);
        }

        public override bool MoveToAttribute(string name)
        {
            return innerReader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return innerReader.MoveToAttribute(name,ns);
        }

        public override bool MoveToElement()
        {
            return innerReader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return innerReader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return innerReader.MoveToNextAttribute();
        }
        
        public override bool Read()
        {
            if (cachedElement != null)
            {
                cachedElement = null;
                return true;
            }

            bool readSuccess = innerReader.Read();
            if (innerReader.NodeType == XmlNodeType.Element &&
                innerReader.HasAttributes &&
                innerReader.Depth == 0)
            {
                throw new System.Exception(Strings.XmlNoRoot);
            }
            if (innerReader.NodeType == XmlNodeType.Element && 
                !innerReader.HasAttributes &&
                innerReader.Depth > 0)
            {
                if (innerReader.IsEmptyElement)
                {
                    return innerReader.Read(); 
                }

                cachedElement = new CachedElement
                {
                    Prefix = innerReader.Prefix,
                    LocalName = innerReader.LocalName,
                    NamespaceURI = innerReader.NamespaceURI,
                    NodeType = XmlNodeType.Element,
                    Value = innerReader.Value,
                    Depth = innerReader.Depth
                };
                readSuccess = innerReader.Read();
                
                if (innerReader.NodeType == XmlNodeType.EndElement)
                {
                    cachedElement = null;
                    return innerReader.Read();
                }
            }

            return readSuccess;
        }

        public override bool ReadAttributeValue()
        {
            return innerReader.ReadAttributeValue();
        }

        public override void ResolveEntity()
        {
            innerReader.ResolveEntity();
        }

        public override void MoveToAttribute(int i)
        {
            innerReader.MoveToAttribute(attributeMappingDictionary[i]);
        }
    }
    
    class CachedElement
    {
        public string Prefix { get; set; }
        public string NamespaceURI { get; set; }
        public string LocalName { get; set; }
        public XmlNodeType NodeType { get; set; }
        public string Value { get; set; }
        public int  Depth { get; set; }
    }
}

