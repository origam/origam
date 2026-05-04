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
using System.Collections;
using System.Text;

namespace Origam;

/// <summary>
/// Summary description for OrigamException.
/// </summary>
public class OrigamException : Exception
{
    private StringBuilder _appendStackTrace;

    public OrigamException()
        : base() { }

    public OrigamException(string message)
        : base(message: message) { }

    public OrigamException(string message, Exception innerException)
        : base(message: message, innerException: innerException)
    {
        foreach (DictionaryEntry entry in innerException.Data)
        {
            Data[key: entry.Key] = entry.Value;
        }
    }

    public OrigamException(string message, string customStackTrace, Exception innerException)
        : this(message: message, innerException: innerException)
    {
        this.AppendStackTrace(stacktrace: customStackTrace);
    }

    public void AppendStackTrace(string stacktrace)
    {
        if (_appendStackTrace == null)
        {
            _appendStackTrace = new StringBuilder();
        }

        _appendStackTrace.Append(value: Environment.NewLine);
        _appendStackTrace.Append(value: "--------------------------------------------");
        _appendStackTrace.Append(value: Environment.NewLine);
        _appendStackTrace.Append(value: stacktrace);
        _appendStackTrace.Append(value: Environment.NewLine);
        _appendStackTrace.Append(value: "--------------------------------------------");
        _appendStackTrace.Append(value: Environment.NewLine);
    }

    public override string StackTrace
    {
        get
        {
            if (_appendStackTrace != null)
            {
                _appendStackTrace.Append(value: base.StackTrace);
                return _appendStackTrace.ToString();
            }

            return base.StackTrace;
        }
    }
}
