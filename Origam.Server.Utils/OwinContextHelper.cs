#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Origam.Server.Utils
{
	public class OwinContextHelper
	{
		private OwinContextHelper() { }
		public static Dictionary<string, string> GetRequestParameters(
			IOwinRequest request)
		{
			if (request.ContentType != null && request.ContentType.Substring(0, 16) == "application/json")
			{
				string body = new StreamReader(request.Body).ReadToEnd();
				Dictionary<string, string> dict
					= JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
				return dict;
			}

			var dictionary = new Dictionary<string, string>(
				StringComparer.CurrentCultureIgnoreCase);
			var formCollectionTask = request.ReadFormAsync();
			formCollectionTask.Wait();
			foreach (var pair in formCollectionTask.Result)
			{
				var value = GetJoinedValue(pair.Value);
				dictionary.Add(pair.Key, value);
			}
			foreach (var pair in request.Headers)
			{
				var value = GetJoinedValue(pair.Value);
				dictionary.Add(pair.Key, value);
			}
			foreach (var pair in request.Query)
			{
				var value = GetJoinedValue(pair.Value);
				dictionary.Add(pair.Key, value);
			}
			return dictionary;
		}

		private static string GetJoinedValue(string[] value)
		{
			if (value != null)
			{
				return string.Join(",", value);
			}
			return null;
		}
	}
}
