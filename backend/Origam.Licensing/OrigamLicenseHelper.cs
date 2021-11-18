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
using System.Linq;
using Microsoft.Win32;
using System.Management;
using Origam.Licensing.Validation;

namespace Origam.Licensing
{
	public class OrigamLicenseHelper
	{
		private const string ORIGAM_COM_KEY_NAME = "Origam";
		private const string ORIGAM_ARCHITECT_KEY_NAME = "Origam Architect";
		private const string LICENSE_KEY_NAME = "License";

		public static void StoreLicenseToRegistry(string License)
		{
			RegistryKey softwareKey = Registry.CurrentUser.CreateSubKey("SOFTWARE");
			RegistryKey origamKey = softwareKey.CreateSubKey(ORIGAM_COM_KEY_NAME);
			RegistryKey origamArchitectKey = origamKey.CreateSubKey(ORIGAM_ARCHITECT_KEY_NAME);
			origamArchitectKey.SetValue(LICENSE_KEY_NAME, License);			
		}

		public static string GetLicenseFromRegistry()
		{
			RegistryKey softwareKey = Registry.CurrentUser.CreateSubKey("SOFTWARE");
			RegistryKey origamKey = softwareKey.CreateSubKey(ORIGAM_COM_KEY_NAME);
			RegistryKey origamArchitectKey = origamKey.CreateSubKey(ORIGAM_ARCHITECT_KEY_NAME);
			return (string) origamArchitectKey.GetValue(LICENSE_KEY_NAME);
		}

		public static Guid GetComputerUUID()
		{
			ManagementClass mc = new ManagementClass("Win32_ComputerSystemProduct");
			ManagementObjectCollection moc = mc.GetInstances();
			String info = String.Empty;
			foreach (ManagementObject mo in moc)
			{
				info = (string)mo["UUID"];
			}
			return string.IsNullOrEmpty(info) ? Guid.Empty : Guid.Parse(info);
		}

		public static List<IValidationFailure> CheckStoredLicenseForExpiration(string licenseString)
		{			
				License license = License.Load(licenseString);
				return
					 license.Validate().ExpirationDate()					
					.AssertValidLicense().ToList();			
        }

		public static List<IValidationFailure> CheckStoredLicense(string licenseString)
		{
			if (string.IsNullOrEmpty(licenseString))
			{
				GeneralValidationFailure gf = new GeneralValidationFailure();
				gf.Message = "License not found.";
				gf.HowToResolve = "Thank you for using ORIGAM Architect!\n\nIn order to provide the best service we will ask you to sign in or register in the next step.";
                return new List<IValidationFailure>() { gf };
			}
			try {
				License license = License.Load(licenseString);
				return
					 license.Validate().Signature()
					.And().ComputerUUID(GetComputerUUID())
					.AssertValidLicenseStopOnFirst().ToList();


			} catch
			{
				GeneralValidationFailure gf = new GeneralValidationFailure();
				gf.Message = "License read error.";
				gf.HowToResolve = "Can't read license. Plase log in to acquire a new license.";
				return new List<IValidationFailure>() {gf};
				// license can't be parsed / isnull

				//return new List<IValidationFailure>() { GeneralValidationFailure }}
			}

		}		
	}
}
