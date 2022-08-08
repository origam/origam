using System;

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
            ChildItemTypes.Add(typeof(XsltFunctionCollection));
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
        
        #endregion
    }
}