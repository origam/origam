using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Origam.DA;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel
{

    public interface IXsltFunctionSchemaItemProvider : ISchemaItemProvider
    {
    }

    public class XsltFunctionSchemaItemProvider : AbstractSchemaItemProvider, IXsltFunctionSchemaItemProvider
    {
        public const string CategoryConst = "XsltFunctionCollection";
		
        public XsltFunctionSchemaItemProvider()
        {
        }

        #region ISchemaItemProvider Members
        public override string RootItemType => CategoryConst;

        public override string Group => "BL";

        #endregion

        #region IBrowserNode Members

        public override string Icon => "icon_31_services.png";

        public override string NodeText
        {
            get => "Xslt Function Collections";
            set => base.NodeText = value;
        }

        public override string NodeToolTipText => null;

        #endregion

        #region ISchemaItemFactory Members

        public override Type[] NewItemTypes
        {
            get
            {
                return new Type[1] {typeof(XsltFunctionCollection)};
            }
        }

        public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
        {
            AbstractSchemaItem item;

            if(type == typeof(XsltFunctionCollection))
            {
                item = new XsltFunctionCollection(schemaExtensionId);
                item.Name = "NewCollection";
            }
            else
                throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorServiceModelUnknownType"));

            item.Group = group;
            item.PersistenceProvider = this.PersistenceProvider;
            item.RootProvider = this;
            this.ChildItems.Add(item);
            return item;
        }

        #endregion
    }
}