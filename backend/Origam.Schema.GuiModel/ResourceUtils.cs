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

using System.Drawing;
using System.Resources;
using System.Threading;

namespace Origam.Schema.GuiModel;

public class ResourceUtils
{
    private static readonly string StringBaseName = "Origam.Schema.GuiModel.Strings";
    private static readonly string ImageBaseName = "Origam.Schema.GuiModel.Images";
    private static ResourceManager stringResmanager;
    private static ResourceManager imageResmanager;

    public static string GetString(string key)
    {
        if (stringResmanager == null)
        {
            stringResmanager = new ResourceManager(
                baseName: StringBaseName,
                assembly: typeof(ResourceUtils).Assembly
            );
        }
        return stringResmanager.GetString(name: key, culture: Thread.CurrentThread.CurrentCulture);
    }

    public static string GetString(string key, params object[] args)
    {
        string rawString = GetString(key: key);
        return string.Format(format: rawString, args: args);
    }

    public static Image GetImage(string key)
    {
        if (imageResmanager == null)
        {
            imageResmanager = new ResourceManager(
                baseName: ImageBaseName,
                assembly: typeof(ResourceUtils).Assembly
            );
        }
        return (Image)imageResmanager.GetObject(name: key);
    }
}
