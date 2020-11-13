
using System;
using System.Collections.Generic;
using System.Text;

namespace Origam.Schema.MenuModel
{
    public class HashTagSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory
    {
        public HashTagSchemaItemProvider()
        {
            this.ChildItemTypes.Add(typeof(HashTag));
        }

		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return HashTag.CategoryConst;
			}
		}
		public override string Group
		{
			get
			{
				return "UI";
			}
		}
		#endregion
		#region IBrowserNode Members

		public override string Icon
		{
			get
			{
				return "icon_23_search-data-sources.png";
			}
		}

		public override string NodeText
		{
			get
			{
				return "Hash Tag";
			}
			set
			{
				base.NodeText = value;
			}
		}

		public override string NodeToolTipText
		{
			get
			{
				return "List of Hash Tags";
			}
		}

		#endregion
	}
}
