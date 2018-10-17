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
