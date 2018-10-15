using System.Collections;
using System.Collections.Generic;
using System.Data;
using Origam;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Gui
{
    public class ExecuteActionProcessData
    {
        public string SessionFormIdentifier { get; set; }

        public string RequestingGrid { get; set; }

        public string ActionId { get; set; }

        public string Entity { get; set; }

        public IList SelectedItems { get; set; }

        public PanelActionType Type { get; set; }

        public UserProfile Profile { get; } = new UserProfile();

        public DataTable DataTable { get; set; }

        public IList<DataRow> Rows { get; set; }

        public IParameterService ParameterService { get; set; }

        public EntityUIAction Action { get; set; } = null;

        public Hashtable Parameters { get; set; } = new Hashtable();
    }
}
