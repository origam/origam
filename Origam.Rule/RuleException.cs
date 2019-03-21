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

namespace Origam.Rule
{
	/// <summary>
	/// Summary description for RuleException.
	/// </summary>
	public class RuleException : Exception
	{
		public RuleException() : base()	{}

		public RuleException(RuleExceptionDataCollection result) : base()
		{
			this.RuleResult.AddRange(result);
		}

        public RuleException(string message)
        {
            this.RuleResult.Add(new RuleExceptionData(message));
        }

		public RuleException(string message, RuleExceptionSeverity severity,
			string fieldName, string entityName)
		{
			this.RuleResult.Add(
				new RuleExceptionData(message, severity, fieldName, entityName)
			);
		}

		public RuleExceptionDataCollection RuleResult = new RuleExceptionDataCollection();

		public override string Message
		{
			get
			{
				string result = "";	

				foreach(RuleExceptionData data in this.RuleResult)
				{
					if(result != "") result += Environment.NewLine;

					result += data.Message; // + ": " + data.EntityName + "." + data.FieldName;
				}

				return result;
			}
		}

		public bool IsSeverityHigh
		{
			get
			{
				foreach(RuleExceptionData data in this.RuleResult)
				{
					if(data.Severity == RuleExceptionSeverity.High)
					{
						return true;
					}
				}

				return false;
			}
		}
	}
}
