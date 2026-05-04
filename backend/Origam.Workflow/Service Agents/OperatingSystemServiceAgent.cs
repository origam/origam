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
using System.Text;
using System.Threading;
using Origam.Service.Core;

namespace Origam.Workflow;

public class OperatingSystemServiceAgent : AbstractServiceAgent
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType
    );
    #region IServiceAgent Members
    private object result;
    public override object Result => result;

    public override void Run()
    {
        result = MethodName switch
        {
            "StartProcess" => StartProcess(
                filename: Parameters.Get<string>(key: "FileName"),
                arguments: Parameters.Get<string>(key: "Arguments"),
                timeout: Parameters.Get<int>(key: "Timeout")
            ),
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(MethodName),
                actualValue: MethodName,
                message: ResourceUtils.GetString(key: "InvalidMethodName")
            ),
        };
    }
    #endregion

    private bool StartProcess(string filename, string arguments, int timeout)
    {
        using var process = new Process();
        process.StartInfo.FileName = filename;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        var standardOutput = new StringBuilder();
        var standardError = new StringBuilder();
        using var outputWaitHandle = new AutoResetEvent(initialState: false);
        using var errorWaitHandle = new AutoResetEvent(initialState: false);
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data == null)
            {
                outputWaitHandle.Set();
            }
            else
            {
                standardOutput.AppendLine(value: e.Data);
            }
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data == null)
            {
                errorWaitHandle.Set();
            }
            else
            {
                standardError.AppendLine(value: e.Data);
            }
        };
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(format: "Executing {0} {1}", arg0: filename, arg1: arguments);
        }
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        if (
            !process.WaitForExit(milliseconds: timeout)
            || !outputWaitHandle.WaitOne(millisecondsTimeout: timeout)
            || !errorWaitHandle.WaitOne(millisecondsTimeout: timeout)
        )
        {
            try
            {
                process.Kill();
            }
            catch
            {
                /* ignore failure */
            }
            throw new Exception(
                message: $"Timeout while executing process: {filename} {arguments}"
            );
        }
        if (standardError.Length > 0)
        {
            throw new Exception(
                message: $"Error while executing process: {filename} {arguments}\n{standardError}"
            );
        }
        return true;
    }
}
