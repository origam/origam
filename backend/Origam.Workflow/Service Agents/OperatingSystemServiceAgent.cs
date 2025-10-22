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

public class OperatingSystemServiceAgent : AbstractServiceAgent
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    #region IServiceAgent Members
    private object _result;
    public override object Result
    {
        get
        {
            object temp = _result;
            _result = null;
            return temp;
        }
    }

    public override void Run()
    {
        switch (this.MethodName)
        {
            case "StartProcess":
            {
                // Check input parameters
                if (!(Parameters["FileName"] is string))
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorPathNotString"));
                }
                if (!(Parameters["Arguments"] is string || Parameters["Arguments"] == null))
                {
                    throw new InvalidCastException(
                        ResourceUtils.GetString("ErrorArgumentsNotString")
                    );
                }
                if (!(Parameters["Timeout"] is int || Parameters["Timeout"] == null))
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorTimeoutNotInt"));
                }
                ProcessStartInfo processStartInfo = new ProcessStartInfo(
                    (string)Parameters["FileName"],
                    (string)Parameters["Arguments"]
                );
                processStartInfo.CreateNoWindow = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardError = true;
                Process process = new Process();
                process.StartInfo = processStartInfo;
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat(
                        "Executing {0} {1}",
                        processStartInfo.FileName,
                        processStartInfo.Arguments
                    );
                }
                process.Start();
                string error = process.StandardError.ReadToEnd();
                if (Parameters["Timeout"] is int)
                {
                    process.WaitForExit((int)Parameters["Timeout"]);
                }
                else
                {
                    process.WaitForExit();
                }
                if (!String.IsNullOrEmpty(error))
                {
                    throw new Exception(
                        ResourceUtils.GetString(
                            "ExternalProcessError",
                            processStartInfo.FileName,
                            error
                        )
                    );
                }
                _result = true;
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    "MethodName",
                    MethodName,
                    ResourceUtils.GetString("InvalidMethodName")
                );
            }
        }
    }
    #endregion
}
