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

using System;
using System.Diagnostics;

namespace Origam.Workflow;

public class PrintServiceAgent : AbstractServiceAgent
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    #region IServiceAgent Members
    private object _result;
    public override object Result
    {
        get { return _result; }
    }

    public override void Run()
    {
        switch (this.MethodName)
        {
            case "PrintPDF":
            {
                if (!(Parameters[key: "Filename"] is string))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorFilenameNotString")
                    );
                }
                if (!(Parameters[key: "Copies"] is int))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorCopiesNotInt")
                    );
                }
                if (!(Parameters[key: "Timeout"] is int))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorTimeoutNotInt")
                    );
                }
                PrintPDF(
                    filename: Parameters[key: "Filename"] as string,
                    printer: Parameters[key: "Printer"] as string,
                    copies: Convert.ToInt32(value: Parameters[key: "Copies"]),
                    paperSource: Parameters[key: "PaperSource"] as string,
                    timeout: Convert.ToInt32(value: Parameters[key: "Timeout"])
                );
                break;
            }
        }
    }
    #endregion
    private void PrintPDF(
        string filename,
        string printer,
        int copies,
        string paperSource,
        int timeout
    )
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        string gsPath = settings.GsPath;
        string arguments;
        if (String.IsNullOrEmpty(value: paperSource))
        {
            arguments = String.Format(
                format: "-dBATCH -dNOPAUSE -dManualFeed=false -sDEVICE=pxlcolor -sOutputFile=\"%printer%{0}\" -c \"<</NumCopies {1}>> setpagedevice\" -f \"{2}\"",
                arg0: printer,
                arg1: copies,
                arg2: filename
            );
        }
        else
        {
            arguments = String.Format(
                format: "-dBATCH -dNOPAUSE -dMediaPosition={3} -dManualFeed=false -sDEVICE=pxlcolor -sOutputFile=\"%printer%{0}\" -c \"<</NumCopies {1}>> setpagedevice\" -f \"{2}\"",
                args: new object[] { printer, copies, filename, paperSource }
            );
        }
        ProcessStartInfo processStartInfo = new ProcessStartInfo(
            fileName: gsPath,
            arguments: arguments
        );
        processStartInfo.CreateNoWindow = false;
        processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardError = true;
        processStartInfo.RedirectStandardOutput = true;
        Process process = new Process();
        process.StartInfo = processStartInfo;
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(
                format: "Printing via ghostscript {0} {1}",
                arg0: processStartInfo.FileName,
                arg1: processStartInfo.Arguments
            );
        }
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit(milliseconds: timeout);
        if (!string.IsNullOrEmpty(value: error))
        {
            throw new Exception(
                message: ResourceUtils.GetString(
                    key: "PrintPDFError",
                    args: new object[] { error, output }
                )
            );
        }
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Printing finished...");
        }
        _result = true;
    }
}
