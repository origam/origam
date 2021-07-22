#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using Origam.UI;
using Origam.Workbench.Pads;

namespace Origam.Workbench.Commands
{
	/// <summary>
	/// Shows the Attachment pad
	/// </summary>
	public class ViewAttachmentPad : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return true;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			AttachmentPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(AttachmentPad)) as AttachmentPad;

			if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
		}		
	}

	/// <summary>
	/// Shows the Attachment pad
	/// </summary>
	public class ViewAuditLogPad : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return true;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			AuditLogPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(AuditLogPad)) as AuditLogPad;

			if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
		}		
	}

	/// <summary>
	/// Shows the Property pad
	/// </summary>
	public class ViewPropertyPad : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return true;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			PropertyPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(PropertyPad)) as PropertyPad;

			if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
		}		
	}

	/// <summary>
	/// Shows the DocumentationPad pad
	/// </summary>
	public class ViewDocumentationPad : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return true;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			DocumentationPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(DocumentationPad)) as DocumentationPad;

			if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
		}		
	}

	/// <summary>
	/// Shows the Extension pad
	/// </summary>
	public class ViewExtensionPad : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return true;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			ExtensionPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(ExtensionPad)) as ExtensionPad;

			if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
		}		
	}

	/// <summary>
	/// Shows the Schema Browser pad
	/// </summary>
	public class ViewSchemaBrowserPad : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return true;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			SchemaBrowser pad = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;

			if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
		}		
	}

	/// <summary>
	/// Shows the Schema Output pad
	/// </summary>
	public class ViewOutputPad : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return true;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			OutputPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(OutputPad)) as OutputPad;

			if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
		}		
	}

	/// <summary>
	/// Shows the Log pad
	/// </summary>
	public class ViewLogPad : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return true;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			LogPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(LogPad)) as LogPad;

			if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
		}		
	}

    /// <summary>
    /// Shows the Server Log pad
    /// </summary>
    public class ViewServerLogPad : AbstractMenuCommand
    {
        public override bool IsEnabled
        {
            get
            {
                return true;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        public override void Run()
        {
            ServerLogPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(ServerLogPad)) as ServerLogPad;

            if (pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
        }
    }
    
    /// <summary>
	/// Shows the Schema Result pad
	/// </summary>
	public class ViewFindSchemaItemResultsPad : AbstractMenuCommand
	{
		public override bool IsEnabled
		{
			get
			{
				return true;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public override void Run()
		{
			FindSchemaItemResultsPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(FindSchemaItemResultsPad)) as FindSchemaItemResultsPad;

			if(pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
		}		
	}

    /// <summary>
	/// Shows the Schema Result pad
	/// </summary>
	public class ViewFindRuleResultsPad : AbstractMenuCommand
    {
        public override bool IsEnabled
        {
            get
            {
                return true;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        public override void Run()
        {
            FindRulesPad pad = WorkbenchSingleton.Workbench.GetPad(typeof(FindRulesPad)) as FindRulesPad;

            if (pad != null) WorkbenchSingleton.Workbench.ShowPad(pad);
        }
    }
}
