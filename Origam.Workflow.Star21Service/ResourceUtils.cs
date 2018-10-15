using System;
using System.Resources;
using System.Threading;

namespace Origam.Workflow.Star21Service
{
	public class ResourceUtils
	{
		private static readonly string BASENAME = "Origam.Workflow.Star21Service.Strings";

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
}
