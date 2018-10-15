using System;
using System.Resources;
using System.Threading;

namespace Origam.workflow.mail
{
	public class ResourceUtils
	{
		private static readonly string BASENAME = "Origam.workflow.mail.Strings";

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
