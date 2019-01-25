using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Origam.Schema.GuiModel;
using Origam.Server;

namespace Origam.ServerCommon
{
    public interface IBasicUIService
    {
        string GetReportStandalone(string reportId, Hashtable parameters,
            DataReportExportFormatType dataReportExportFormatType);

        UIResult InitUI(UIRequest request);
    }
}
