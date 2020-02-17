using System.IO;
using Origam.Extensions;

namespace Origam.DA.Service
{
    /// <summary>
    ///  This class wraps DirectoryInfo because we need it as a key in a
    ///  dictionary and DirectoryInfo doesn't override hashcode ands Equals.
    /// </summary>
    public class Folder
    {
        private readonly DirectoryInfo dirInfo;

        public Folder(string path)
        {
            dirInfo = new DirectoryInfo(path); 
        }

        public Folder Parent => new Folder(dirInfo.Parent.FullName);

        public bool IsParentOf(Folder other) => 
            dirInfo.IsOnPathOf(other.dirInfo);

        private bool Equals(Folder other) => 
            string.Equals(dirInfo.FullName, other.dirInfo.FullName);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Folder) obj);
        }

        public override int GetHashCode() => 
            (dirInfo.FullName != null ? dirInfo.FullName.GetHashCode() : 0);
    }
}