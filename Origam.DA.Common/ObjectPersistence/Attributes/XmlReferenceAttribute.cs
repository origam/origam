using System;
using Origam.OrigamEngine;

namespace Origam.DA.ObjectPersistence
{
	/// <summary>
	/// Use this attribute to identify a Guid type field which stores a reference
	/// to a parent object. This value will not be serialized to the model storage
    /// but when retrieving it will be passed from a parent object.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple=false, Inherited=true)]
	public class XmlReferenceAttribute : Attribute
	{
		/// <summary>
        /// The constructor for the XmlParent attribute.
        /// </summary>
        /// <param name="type">The type that is referenced.</param>
        public XmlReferenceAttribute(string attributeName, string idField)
		{
            AttributeName = attributeName;
			IdField = idField;
		}

		/// <summary>
		/// Field which contains a referenced object's id
		/// </summary>
		public string IdField { get; }

		public string AttributeName { get; }

		public string Namespace => null;
	}

	public class XmlPackageReferenceAttribute: XmlReferenceAttribute
	{
		public XmlPackageReferenceAttribute(string attributeName, string idField) : base(attributeName, idField)
		{
			
		}
		public string Namespace =>
			$"http://schemas.origam.com/{VersionProvider.CurrentPackageMeta}/package";
	}
}
