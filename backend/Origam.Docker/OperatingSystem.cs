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

using Docker.DotNet.Models;
using System;
using System.Threading.Tasks;

namespace Origam.Docker;
public class OperatingSystem
{
    public string TextOs { get; }
    public string FileNameExtension => GetFileNameExtension();
    public OperatingSystem(string textOs)
    {
        TextOs = textOs;
    }
    public OperatingSystem(Task<VersionResponse> task)
    {
        TextOs = task.ConfigureAwait(false).GetAwaiter().GetResult().Os;
    }
    private string GetFileNameExtension()
    {
        switch (TextOs)
        {
            case "linux":
                return ".sh";
            case "Windows":
                return ".cmd";
            case "Apple":
                return ".sh";
            default:
                throw new Exception("Docker Operation System was not recognize.");
        }
    }
    public bool IsOnline()
    {
        return string.IsNullOrEmpty(TextOs);
    }
}
