#region  NandoF library -- Copyright 2005-2006 Nando Florestan
/*
This library is free software; you can redistribute it and/or modify
it under the terms of the Lesser GNU General Public License as published by
the Free Software Foundation; either version 2.1 of the License, or
(at your option) any later version.

This software is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with this program; if not, see http://www.gnu.org/copyleft/lesser.html
 */
#endregion

using ConfigurationSettings       = System.Configuration.ConfigurationSettings;
using ArgumentNullException       = System.ArgumentNullException;

namespace NandoF.Web
{
	/// <summary>Represents the path to a file on a webserver. Keeps the
	/// physical path synchronized to the virtual path. The path can also be
	/// discovered in the .config file.</summary>
	public class WebFileLocation
	{
		protected readonly object  pathLock = new object();
		public    readonly string  DefaultVirtualPath;
		public    readonly string  AppSettingsVirtualKey;
		public    readonly string  AppSettingsPhysicalKey;
		
		protected string  virtualPath;
		public    string  VirtualPath    {
			set  {
				lock (pathLock)  {
					if (value==null || value==string.Empty)  {
						// User or constructor is trying to set virtualPath to null.
						// Before we let this happen, try to find the path.
						setFromConfigs();
					}
					else  {
						virtualPath = value;
						setPhysicalFromVirtual();
					}
				}
			}
			get  {
				lock (pathLock)  {
					if (virtualPath==null)  throw new System.ApplicationException
						("VirtualPath is null.");
					else  return virtualPath;
				}
			}
		}
		
		protected void    setPhysicalFromVirtual()  {
			if (virtualPath != null && virtualPath != string.Empty)  physicalPath =
				System.Web.HttpContext.Current.Request.MapPath(virtualPath);
			else {
				physicalPath = null;
			}
		}
		
		protected string  physicalPath;
		public    string  PhysicalPath  {
			set  {
				lock (pathLock)  {
					if (value==null || value==string.Empty)  setPhysicalFromVirtual();
					else  {
						physicalPath = value;
						virtualPath  = null;
					}
				}
			}
			get  {
				lock (pathLock)  {
					if (physicalPath==null)  throw new System.ApplicationException
						("PhysicalPath is null.");
					else  return physicalPath;
				}
			}
		}
		
		protected void    setFromConfigs()  {
			// 1) See if PHYSICAL LOCATION is specified in the web.config file
			string conf = ConfigurationSettings.AppSettings[AppSettingsPhysicalKey];
			if (conf != null  &&  conf != string.Empty)  {
				physicalPath = conf;
				virtualPath  = null;
				return;
			}
			// 2) See if VIRTUAL LOCATION is specified in the web.config file
			conf = ConfigurationSettings.AppSettings[AppSettingsVirtualKey];
			if (conf != null  &&  conf != string.Empty)  {
				virtualPath = conf;
				setPhysicalFromVirtual();
				return;
			}
			// 3) Else, use the default
			if (DefaultVirtualPath != null && DefaultVirtualPath != string.Empty)  {
				virtualPath = DefaultVirtualPath;
				setPhysicalFromVirtual();
				return;
			}
			// 4) You might as well give up
			virtualPath  = null;
			physicalPath = null;
		}
		
		public /*constructor*/ WebFileLocation(string defaultVirtualPath,
		                                       string appSettingsVirtualKey,
		                                       string appSettingsPhysicalKey)  {
			if (appSettingsVirtualKey==null  || appSettingsVirtualKey==string.Empty)
				throw new ArgumentNullException("appSettingsVirtualKey");
			if (appSettingsPhysicalKey==null || appSettingsPhysicalKey==string.Empty)
				throw new ArgumentNullException("appSettingsPhysicalKey");
			this.DefaultVirtualPath     = defaultVirtualPath;
			this.AppSettingsVirtualKey  = appSettingsVirtualKey;
			this.AppSettingsPhysicalKey = appSettingsPhysicalKey;
			// Setting VirtualPath to null starts a chain reaction,
			// makes it try to find the path
			this.VirtualPath = null;
		}
		
		public /*constructor*/ WebFileLocation(string physicalPath)  {
			if (physicalPath==null || physicalPath==string.Empty)  throw new
				ArgumentNullException("physicalPath");
			PhysicalPath = physicalPath;
		}
		
		public string GetErrorIfDirDoesNotExist()  {
			string dir = System.IO.Path.GetDirectoryName(PhysicalPath);
			if (System.IO.Directory.Exists(dir))  return null;
			else  return "Directory does not exist: " + dir;
		}
	}
}
