using System;

namespace Origam.DA
{

//    public class ElementName
//    {
//        private readonly string value;
//
//        public ElementName(string value)
//        {
//            if (!value.StartsWith("http://schemas.origam.com"))
//            {
//                throw new ArgumentException(nameof(ElementName)+" must start with http://schemas.origam.com");
//            }
//            if (!Uri.IsWellFormedUriString(value, UriKind.Absolute))
//            {
//                throw new ArgumentException(value +" is not a valid absolute Uri");
//            }
//            this.value = value;
//        }
//
//        public override string ToString() => value;
//        
//        public static implicit operator string(ElementName folderUri) => 
//            folderUri.ToString();
//
//        protected bool Equals(ElementName other) => string.Equals(value, other.value);
//
//        public override bool Equals(object obj)
//        {
//            if (ReferenceEquals(null, obj)) return false;
//            if (ReferenceEquals(this, obj)) return true;
//            if (obj.GetType() != this.GetType()) return false;
//            return Equals((ElementName) obj);
//        }
//
//        public override int GetHashCode() => (value != null ? value.GetHashCode() : 0);
//    }

}