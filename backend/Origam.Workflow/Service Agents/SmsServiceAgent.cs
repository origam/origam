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

namespace Origam.Workflow.Service_Agents
{
    public class SmsServiceAdapter : AbstractServiceAgent
    {
        private object _result;
        public override object Result
        {
            get
            {
                var temp = _result;
                _result = null;
				
                return temp;
            }
        }
        public override void Run()
        {
            switch (MethodName)
            {
                case "SendSms":
                    if(! (this.Parameters["from"] is null))
                        throw new InvalidCastException(ResourceUtils.GetString("Parameter from cannot be null"));
                    if(! (this.Parameters["to"] is null))
                        throw new InvalidCastException(ResourceUtils.GetString("Parameter to cannot be null"));
                    if(! (this.Parameters["body"] is null))
                        throw new InvalidCastException(ResourceUtils.GetString("Parameter body cannot be null"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("MethodName", this.MethodName, ResourceUtils.GetString("InvalidMethodName"));
            }
        }
    }
}