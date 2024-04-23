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

namespace Origam;

/// <summary>
/// Summary description for Debug.
/// </summary>
public class Debug
{
	protected Debug()
	{
			//
			// TODO: Add constructor logic here
			//
		}

	private static Hashtable _counters = new Hashtable();
	public static void UpdateCounter(string counterName, int offset)
	{
			if(! Debug._counters.ContainsKey(counterName))
			{
				Debug._counters.Add(counterName, 0);
			}

			Debug._counters[counterName] = (int)Debug._counters[counterName] + offset;
		}

	public static void PrintCounters()
	{
			System.Diagnostics.Debug.WriteLine("******************* COUNTERS *****************");
			foreach(DictionaryEntry entry in Debug._counters)
			{
				System.Diagnostics.Debug.WriteLine(entry.Key.ToString() + ": " + entry.Value.ToString());
			}
			System.Diagnostics.Debug.WriteLine("***************** COUNTERS END ***************");
		}
}