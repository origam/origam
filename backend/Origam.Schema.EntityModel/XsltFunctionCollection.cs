using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel
{
	[SchemaItemDescription("XsltFunctionCollection", "service.png")]
	[HelpTopic("Services")]
	[XmlModelRoot(CategoryConst)]
	[ClassMetaVersion("6.0.0")]
	public class XsltFunctionCollection : AbstractSchemaItem
	{
		public const string CategoryConst = "XsltFunctionCollection";
		public override string ItemType => CategoryConst;

		public XsltFunctionCollection()
		{
		}

		public XsltFunctionCollection(Guid schemaExtensionId) : base(schemaExtensionId)
		{
		}

		public XsltFunctionCollection(Key primaryKey) : base(primaryKey)
		{
		}

		[StringNotEmptyModelElementRule]
		[Description("C# namespace followed by a \".\" and a class name.")]
		[XmlAttribute("fullClassName")]
		public string FullClassName { get; set; }
		
		[StringNotEmptyModelElementRule]
		[Description("Assembly name (without extension) where the class is defined.")]
		[XmlAttribute("assemblyName")]
		public string AssemblyName { get; set; }
		
		[StringNotEmptyModelElementRule]
		[Description("Xslt functions found in the provided class will be defined in this xslt namespace.")]
		[XmlAttribute("xslNameSpaceUri")]
		public string XslNameSpaceUri { get; set; }	
		
		[StringNotEmptyModelElementRule]
		[Description("Xslt namespace prefix in xpath. Prefix in Xslt transformations will be declared in the Xslt templates.")]
		[XmlAttribute("xslNameSpacePrefix")]
		public string XslNameSpacePrefix { get; set; }
	}
}