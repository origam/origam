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

using System.Resources;
using System.Threading;

namespace Origam.Excel;
public class ResourceUtils
{
	private static readonly string BASENAME = "Origam.Excel.Strings";
	private static ResourceManager _rm = null;
	
	public static string GetString(string key)
	{
		if (_rm == null) 
		{
			_rm = new ResourceManager(BASENAME, typeof(ResourceUtils).Assembly);
		}
		return _rm.GetString(key, Thread.CurrentThread.CurrentCulture);
	}
	public static string GetString(string key, params object[] args)
	{
		string rawString = GetString(key);
		return string.Format(rawString, args);
	}
}
