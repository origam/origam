using System;
using ProtoBuf;

namespace Origam.DA.Service
{
    [ProtoContract]
    public class TypeInfo
    {
        [ProtoMember(1)]
        public string FullTypeName { get;  }

        public Version Version {
            get
            {
                if (version == null)
                {
                    version = Version.Parse(versionStr);
                }
                return version;
            }
        }

        private Version version;

        [ProtoMember(2)]
        private readonly string versionStr;

        public TypeInfo()
        {
        }

        public TypeInfo( string fullTypeName, Version version)
        {
            versionStr =  version.ToString();
            FullTypeName = fullTypeName;
        }

        protected bool Equals(TypeInfo other)
        {
            return versionStr == other.versionStr && FullTypeName == other.FullTypeName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TypeInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((versionStr != null ? versionStr.GetHashCode() : 0) * 397) ^ (FullTypeName != null ? FullTypeName.GetHashCode() : 0);
            }
        }
    }
}