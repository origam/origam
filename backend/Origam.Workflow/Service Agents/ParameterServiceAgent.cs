#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using Origam.Workbench.Services;

namespace Origam.Workflow;

public class ParameterServiceAgent : AbstractServiceAgent
{
    object _result = null;
    public override object Result
    {
        get { return _result; }
    }

    public override void Run()
    {
        switch (this.MethodName)
        {
            case "SetCustomParameterValue":
                // Check input parameters
                if (!(this.Parameters["ParameterName"] is string))
                    throw new InvalidCastException("ParameterName has to be string");
                object value = this.Parameters["Value"];
                IParameterService paramSvc =
                    ServiceManager.Services.GetService(typeof(IParameterService))
                    as IParameterService;
                int intValue = 0;
                string stringValue = "";
                Guid guidValue = Guid.Empty;
                bool boolValue = false;
                decimal decimalValue = 0;
                object dateValue = null;
                if (value is int)
                {
                    intValue = (int)value;
                }
                else if (value is string)
                {
                    stringValue = (string)value;
                }
                else if (value is Guid)
                {
                    guidValue = (Guid)value;
                }
                else if (value is decimal)
                {
                    decimalValue = (decimal)value;
                }
                else if (value is bool)
                {
                    boolValue = (bool)value;
                }
                else if (value is DateTime)
                {
                    dateValue = value;
                }
                object profileIdParam = Parameters["ProfileId"];
                Guid? profileId;
                if (profileIdParam is string)
                {
                    profileId = new Guid((string)profileIdParam);
                }
                else if (profileIdParam is Guid)
                {
                    profileId = (Guid)profileIdParam;
                }
                else if (profileIdParam == null)
                {
                    profileId = null;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(
                        "ProfileId",
                        profileIdParam,
                        "ProfileId must be Guid, string or empty."
                    );
                }
                paramSvc.SetCustomParameterValue(
                    (string)Parameters["ParameterName"],
                    value,
                    guidValue,
                    intValue,
                    stringValue,
                    boolValue,
                    decimalValue,
                    decimalValue,
                    dateValue,
                    profileId
                );
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    "MethodName",
                    this.MethodName,
                    ResourceUtils.GetString("InvalidMethodName")
                );
        }
    }
}
