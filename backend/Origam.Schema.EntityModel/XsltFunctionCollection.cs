using System;
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
		public override string ItemType { get; }

		public XsltFunctionCollection()
		{
		}

		public XsltFunctionCollection(Guid schemaExtensionId) : base(schemaExtensionId)
		{
		}

		public XsltFunctionCollection(Key primaryKey) : base(primaryKey)
		{
		}
		

		[XmlAttribute("fullClassName")]
		public string FullClassName { get; set; }
		
		[XmlAttribute("assemblyName")]
		public string AssemblyName { get; set; }
		
		[XmlAttribute("xslNameSpaceUri")]
		public string XslNameSpaceUri { get; set; }	
		
		[XmlAttribute("xslNameSpacePrefix")]
		public string XslNameSpacePrefix { get; set; }
	}
}