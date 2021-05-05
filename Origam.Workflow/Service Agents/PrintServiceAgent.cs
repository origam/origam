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

namespace Origam.Workflow
{
    public class PrintServiceAgent : AbstractServiceAgent
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region IServiceAgent Members
        private object _result;
        public override object Result
        {
            get
            {
                return _result;
            }
        }

        public override void Run()
        {
            switch (this.MethodName)
            {
                case "PrintPDF":
                    if (!(Parameters["Filename"] is string))
                    {
						throw new InvalidCastException(
                            ResourceUtils.GetString("ErrorFilenameNotString"));
                    }
                    if (!(Parameters["Copies"] is int))
                    {
                        throw new InvalidCastException(
                            ResourceUtils.GetString("ErrorCopiesNotInt"));
                    }
					if(!(Parameters["Timeout"] is int))
                    {
						throw new InvalidCastException(
                            ResourceUtils.GetString("ErrorTimeoutNotInt"));
                    }
                    PrintPDF(
                        Parameters["Filename"] as string,
                        Parameters["Printer"] as string,
                        Convert.ToInt32(Parameters["Copies"]),
                        Parameters["PaperSource"] as string,
                        Convert.ToInt32(Parameters["Timeout"]));
                    break;
            }
        }
        #endregion

        private void PrintPDF(
            string filename, string printer,
            int copies, string paperSource, int timeout)
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            string gsPath = settings.GsPath;

            string arguments;
            if (String.IsNullOrEmpty(paperSource))
            {
                arguments = String.Format("-dBATCH -dNOPAUSE -dManualFeed=false -sDEVICE=pxlcolor -sOutputFile=\"%printer%{0}\" -c \"<</NumCopies {1}>> setpagedevice\" -f \"{2}\"",
                    printer, copies, filename);
            }
            else
            {
                arguments = String.Format("-dBATCH -dNOPAUSE -dMediaPosition={3} -dManualFeed=false -sDEVICE=pxlcolor -sOutputFile=\"%printer%{0}\" -c \"<</NumCopies {1}>> setpagedevice\" -f \"{2}\"",
                    printer, copies, filename, paperSource);
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo(
                gsPath, arguments);
            processStartInfo.CreateNoWindow = false;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            Process process = new Process();
            process.StartInfo = processStartInfo;
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Printing via ghostscript {0} {1}", 
                    processStartInfo.FileName, 
                    processStartInfo.Arguments);
            }
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit(timeout);
            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(ResourceUtils.GetString(
                    "PrintPDFError", error, output));
            }
            if (log.IsDebugEnabled)
            {
                log.Debug("Printing finished...");
            }
            _result = true;
        }
    }
}
