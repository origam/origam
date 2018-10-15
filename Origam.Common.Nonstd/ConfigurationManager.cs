#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

namespace Origam
{
	/// <summary>
	/// Summary description for ConfigurationManager.
	/// </summary>
	public class ConfigurationManager
	{
		//todo: review whether object is necessary, through the whole code only OrigamSettings is used and unnecessary casts are performed
		private static object _activeConfiguration;

		public static void SetActiveConfiguration(object configuration)
		{
			_activeConfiguration = configuration;
		}

		public static object GetActiveConfiguration()
		{
			return _activeConfiguration;
		}

		public static OrigamSettingsCollection GetAllConfigurations(string name)
		{
			object settings = Microsoft.Practices.EnterpriseLibrary.Configuration.ConfigurationManager.GetConfiguration(name);

			if(settings is OrigamSettings origamSettings)
			{
                origamSettings.BaseFolder = System.Windows.Forms.Application.StartupPath;
				// upgrade
				OrigamSettingsCollection collection = new OrigamSettingsCollection();
				collection.Add(origamSettings);
				WriteConfiguration("OrigamSettings", collection);

				return collection;
			}
			if(settings is OrigamSettingsCollection orSettingsCollection)
			{
				return orSettingsCollection;
			}
			else
			{
				throw new Exception(ResourceUtils.GetString("SettingsInvalidFormat"));
			}
		}

		public static void WriteConfiguration(string name, object configuration)
		{
			if(configuration is OrigamSettingsCollection)
			{
				// do some sanity check

				SortedList list = new SortedList();
				foreach(OrigamSettings setting in (configuration as OrigamSettingsCollection))
				{
					if(!list.Contains(setting.Name))
					{
						list.Add(setting.Name, setting);
					}
					else
					{
						throw new Exception(ResourceUtils.GetString("CantSaveConfig"));
					}
				}
			}

			Microsoft.Practices.EnterpriseLibrary.Configuration.ConfigurationManager.WriteConfiguration(name, configuration);
		}
	}
}
