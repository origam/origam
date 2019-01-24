using System.Collections;
using Origam.Schema.GuiModel;

namespace Origam.Server
{
    public interface IReportManager
    {
        string GetReport(string sessionFormIdentifier, string entity,
            object id, string reportId, Hashtable parameterMappings);

        string GetReportStandalone(string reportId, Hashtable parameters,
            DataReportExportFormatType dataReportExportFormatType);
        string GetReportFromMenu(string menuId);
    }
}