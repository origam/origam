#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Xml;
using Origam.Workflow;
using Origam.Licensing.Validation;

namespace Origam.Licensing.Service
{
    public class OrigamLicensingService : AbstractServiceAgent
	{
		public OrigamLicensingService() { }

		private object _result;
		public override object Result
		{
			get
			{
				object temp = _result;
				_result = null;
				return temp;
			}
		}

		public override void Run()
		{
			switch (this.MethodName)
			{
				case "ExtendLicense":
					if (!(this.Parameters["CurrentLicenseString"] is String))
						throw new InvalidCastException(ResourceUtils.GetString(
							"ErrorInvalidParameterType", "CurrentLicense", "String"));
					if (!(this.Parameters["ExpirationDate"] is DateTime))
						throw new InvalidCastException(ResourceUtils.GetString(
							"ErrorInvalidParameterType", "ExpirationDate", "DateTime"));
					// check license

					XmlDocument currentLicenseXmlDoc = new XmlDocument();
					currentLicenseXmlDoc.LoadXml((string)this.Parameters["CurrentLicenseString"]);
					License currentLicense = License.Load(new XmlNodeReader(currentLicenseXmlDoc));

					List<IValidationFailure> failures =
						currentLicense.Validate().Signature()
						.AssertValidLicense().ToList();

					if (failures.Count() > 0)
					{
						throw new Origam.Rule.RuleException(
							new Origam.Rule.RuleExceptionDataCollection
							(
								failures.Select(x => x.RuleException()).ToArray()
							)
						);						
					}
					
					/*
					if (failures.Count() > 0)
					{


						XElement xmlData = new XElement("Errors");
						failures.ForEach(x => xmlData.Add(x.GetXElement()));
						_result = xmlData.ToString();
						break;
					}
					*/

					// alter licence's expiration
					currentLicense.RemoveSignature();
					currentLicense.Expiration = (DateTime)this.Parameters["ExpirationDate"];
					currentLicense.Sign();

					_result = currentLicense.ToString();
					break;
				case "GenerateLicense":
					if (!(this.Parameters["Email"] is String))
						throw new InvalidCastException(ResourceUtils.GetString(
							"ErrorInvalidParameterType", "Email", "String"));
					if (!(this.Parameters["ExpirationDate"] is DateTime))
						throw new InvalidCastException(ResourceUtils.GetString(
							"ErrorInvalidParameterType", "ExpirationDate", "DateTime"));
					if (!(this.Parameters["FirstName"] is String))
						throw new InvalidCastException(ResourceUtils.GetString(
							"ErrorInvalidParameterType", "FirstName", "String"));
					if (!(this.Parameters["Name"] is String))
						throw new InvalidCastException(ResourceUtils.GetString(
							"ErrorInvalidParameterType", "Name", "String"));
					if (!(this.Parameters["ComputerUUID"] is Guid))
						throw new InvalidCastException(ResourceUtils.GetString(
							"ErrorInvalidParameterType", "ComputerUUID", "Guid"));
					Guid licenceId = Guid.NewGuid();
					if (this.Parameters.Contains("LicenseId"))
					{
						if (!(this.Parameters["LicenseId"] is Guid))
							throw new InvalidCastException(ResourceUtils.GetString(
								"ErrorInvalidParameterType", "LicenseId", "Guid"));
						licenceId = (Guid)this.Parameters["LicenseId"];
					}


					ILicenseBuilder licenseBuilder = new LicenseBuilder()
						.As(LicenseType.Standard)
						.WithUniqueIdentifier(licenceId)
						.ExpiresAt((DateTime)this.Parameters["ExpirationDate"])
						.LicensedTo(String.Format("{0} {1}",
							(string)this.Parameters["FirstName"],
							(string)this.Parameters["Name"]),
							(string)this.Parameters["Email"])
						.WithAdditionalAttributes(new Dictionary<string, string>()
						{
							{"ComputerUUID", ((Guid)this.Parameters["ComputerUUID"]).ToString() }
						})		
						.WithProductFeatures(new Dictionary<string, string>()
						{
							{ "OrigamArchitect", "1" }
						})
					;

					_result = licenseBuilder.CreateAndSignWithPrivateKey().ToString();
					break;
					throw new ArgumentOutOfRangeException("MethodName", this.MethodName, ResourceUtils.GetString("InvalidMethodName"));
				}
			}
		}
	}
