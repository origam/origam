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

using System.Linq;
using Origam.Rule;
using System.Xml.Linq;

namespace Origam.Licensing.Validation
{
	public static class RuleExceptionDataExtensions
	{
		public static RuleExceptionData RuleException(
			this IValidationFailure inFailure)
		{
			return new RuleExceptionData(inFailure.HowToResolve,
				RuleExceptionSeverity.High,
				inFailure.GetType().ToString().Split('.').Last(), "");
		}

		public static XElement GetXElement(
			this IValidationFailure inFailure)
		{
			XElement element = new XElement("Error");
			element.SetAttributeValue("Message", inFailure.Message);
			element.SetAttributeValue("ErrorId", inFailure.GetType().ToString().Split('.').Last());
			return element;
		}
	}
}
