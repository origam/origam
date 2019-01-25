using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Origam.Schema.GuiModel;
using Origam.Server;

namespace Origam.ServerCommon
{
    public class BasicUiService: IBasicUIService
    {
        public string GetReportStandalone(string reportId, Hashtable parameters,
            DataReportExportFormatType dataReportExportFormatType)
        {
            return "";
        }

        public UIResult InitUI(UIRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
