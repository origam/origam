using System.Collections.Generic;
using System.Xml;

namespace Origam
{
    public class XmlReaderCore : XmlReader
    {
        private Dictionary<int, int> dictionary;
        private readonly XmlReader innerReader;
        public XmlReaderCore(XmlReader reader)
        {
            innerReader = reader;
        }
        public override int AttributeCount
        {
            get
            {
                dictionary = new Dictionary<int, int>();
                int realPosition = 0;
                int fakePosition = 0;
                while (MoveToNextAttribute())
                {
                    if (!string.IsNullOrEmpty(Value))
                    {
                        dictionary.Add(fakePosition, realPosition);
                        fakePosition++;
                    }
                    realPosition++;
                }
                return dictionary.Count;
            }
        }

        public override string BaseURI => innerReader.BaseURI;

        public override int Depth => innerReader.Depth;

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
            return innerReader.GetAttribute(dictionary[i]);
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
        
        private CachedElement cachedElement;
        
        public override bool Read()
        {
            if (cachedElement != null)
            {
                cachedElement = null;
                return true;
            }

            bool readSuccess = innerReader.Read();
            if (innerReader.NodeType == XmlNodeType.Element && 
                !innerReader.HasAttributes &&
                innerReader.Depth > 0)
            {
                cachedElement = new CachedElement
                {
                    Prefix = innerReader.Prefix,
                    LocalName = innerReader.LocalName,
                    NamespaceURI = innerReader.NamespaceURI,
                    NodeType = XmlNodeType.Element,
                    Value = innerReader.Value
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
            innerReader.MoveToAttribute(dictionary[i]);
        }
    }
    
    class CachedElement
    {
        public string Prefix { get; set; }
        public string NamespaceURI { get; set; }
        public string LocalName { get; set; }
        public XmlNodeType NodeType { get; set; }
        public string Value { get; set; }
    }
}

