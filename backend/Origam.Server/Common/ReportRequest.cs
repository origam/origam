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

using System.Collections;
using Origam.Schema.GuiModel;

namespace Origam.Server;

public class ReportRequest
{
    public ReportRequest(string reportId, Hashtable parameters)
        : this(reportId, parameters, DataReportExportFormatType.PDF) { }

    public ReportRequest(
        string reportId,
        Hashtable parameters,
        DataReportExportFormatType dataReportExportFormatType
    )
    {
        _reportId = reportId;
        _parameters = parameters;
        UserName = SecurityManager.CurrentPrincipal.Identity.Name;
        _dataReportExportFormatType = dataReportExportFormatType;
    }

    private string _reportId;
    public string ReportId
    {
        get { return _reportId; }
        set { _reportId = value; }
    }
    private DataReportExportFormatType _dataReportExportFormatType;
    public DataReportExportFormatType DataReportExportFormatType
    {
        get { return _dataReportExportFormatType; }
    }
    private Hashtable _parameters;
    public Hashtable Parameters
    {
        get { return _parameters; }
        set { _parameters = value; }
    }
    private int _timesRequested;
    public int TimesRequested
    {
        get { return _timesRequested; }
        set { _timesRequested = value; }
    }
    public string UserName { get; set; }
}
