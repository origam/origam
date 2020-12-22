using System.Collections.Generic;
using System.Xml;

namespace Origam
{
    public class XmlReaderCore : XmlReader
    {
        private Dictionary<int, int> dictionary;
        private readonly XmlReader XmlReader;
        public XmlReaderCore(XmlReader reader)
        {
            this.XmlReader = reader;
        }
        public override int AttributeCount
        {
            get
            {
                dictionary = new Dictionary<int, int>();
                int realposition = 0;
                int fakeposition = 0;
                while (this.MoveToNextAttribute())
                {
                    if (!string.IsNullOrEmpty(Value))
                    {
                        dictionary.Add(fakeposition, realposition);
                        fakeposition++;
                    }
                    realposition++;
                }
                return dictionary.Count;
            }
        }

        public override string BaseURI => this.XmlReader.BaseURI;

        public override int Depth => this.XmlReader.Depth;

        public override bool EOF => this.XmlReader.EOF;

        public override bool IsEmptyElement => this.XmlReader.IsEmptyElement;

        public override string LocalName => 
            cachedElement?.LocalName ?? this.XmlReader.LocalName;

        public override string NamespaceURI => 
            cachedElement?.NamespaceURI ?? this.XmlReader.NamespaceURI;

        public override XmlNameTable NameTable => this.XmlReader.NameTable;

        public override XmlNodeType NodeType => 
            cachedElement?.NodeType ?? this.XmlReader.NodeType;

        public override string Prefix => 
            cachedElement?.Prefix ?? this.XmlReader.Prefix;

        public override ReadState ReadState => this.XmlReader.ReadState;

        public override string Value => 
            cachedElement?.Value ?? this.XmlReader.Value;

        public override string GetAttribute(int i)
        {
            return this.XmlReader.GetAttribute(dictionary[i]);
        }

        public override string GetAttribute(string name)
        {
            return this.XmlReader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return this.XmlReader.GetAttribute(name,namespaceURI);
        }

        public override string LookupNamespace(string prefix)
        {
            return this.XmlReader.LookupNamespace(prefix);
        }

        public override bool MoveToAttribute(string name)
        {
            return this.XmlReader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return this.XmlReader.MoveToAttribute(name,ns);
        }

        public override bool MoveToElement()
        {
            return this.XmlReader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return this.XmlReader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return this.XmlReader.MoveToNextAttribute();
        }
        
        private CachedElement cachedElement;
        
        public override bool Read()
        {
            if (cachedElement != null)
            {
                cachedElement = null;
                return true;
            }

            bool readSuccess = XmlReader.Read();
            if (XmlReader.NodeType == XmlNodeType.Element && 
                !XmlReader.HasAttributes &&
                XmlReader.Depth > 0)
            {
                cachedElement = new CachedElement
                {
                    Prefix = XmlReader.Prefix,
                    LocalName = XmlReader.LocalName,
                    NamespaceURI = XmlReader.NamespaceURI,
                    NodeType = XmlNodeType.Element,
                    Value = XmlReader.Value
                };
                readSuccess = XmlReader.Read();
                
                if (XmlReader.NodeType == XmlNodeType.EndElement)
                {
                    cachedElement = null;
                    return XmlReader.Read();
                }
            }
            
            return readSuccess;
        }

        public override bool ReadAttributeValue()
        {
            return this.XmlReader.ReadAttributeValue();
        }

        public override void ResolveEntity()
        {
            this.XmlReader.ResolveEntity();
        }

        public override void MoveToAttribute(int i)
        {
            this.XmlReader.MoveToAttribute(dictionary[i]);
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

