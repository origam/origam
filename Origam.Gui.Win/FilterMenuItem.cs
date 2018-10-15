using System;
using System.Windows.Forms;

using Origam.Schema.EntityModel;

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for FilterMenuItem.
	/// </summary>
	public class FilterMenuItem : MenuItem
	{
		public FilterMenuItem(string name) : base(name)	{}

		private OrigamPanelFilter.PanelFilterRow _filter;
		public OrigamPanelFilter.PanelFilterRow Filter
		{
			get
			{
				return _filter;
			}
			set
			{
				_filter = value;
			}
		}
	}

	public class TemplateMenuItem : MenuItem
	{
		public TemplateMenuItem(string name) : base(name)	{}

		public TemplateMenuItem(string name, DataStructureTemplate template) : base(name)
		{
			this.Template = template;
		}

		private DataStructureTemplate _template;
		public DataStructureTemplate Template
		{
			get
			{
				return _template;
			}
			set
			{
				_template = value;
			}
		}
	}
}
