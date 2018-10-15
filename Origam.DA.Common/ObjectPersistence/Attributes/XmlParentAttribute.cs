using System;

namespace Origam.DA.ObjectPersistence
{
	/// <summary>
	/// Use this attribute to identify a Guid type field which stores a reference
	/// to a parent object. This value will not be serialized to the model storage
    /// but when retrieving it will be passed from a parent object.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple=false, Inherited=true)]
	public sealed class XmlParentAttribute : Attribute
	{
		private Type _type;

        /// <summary>
        /// The constructor for the XmlParent attribute.
        /// </summary>
        /// <param name="type">The type that is referenced.</param>
        public XmlParentAttribute(Type type)
		{
			_type = type;
		}

		/// <summary>
		/// The type referenced.
		/// </summary>
		public Type Type 
		{
			get{return _type;}
		}
	}
}
