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
using Origam.Service.Core;
using Origam.Sms;

namespace Origam.Workflow
{
    public class SmsServiceAdapter : AbstractServiceAgent
    {
        private object result;
        public override object Result => result;
        public override void Run()
        {
            switch (MethodName)
            {
                case "SendSms":
                    result = CreateSmsService().SendSms(
                        Parameters.Get<string>("from"),
                        Parameters.Get<string>("to"),
                        Parameters.Get<string>("body"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "MethodName", MethodName,
                        ResourceUtils.GetString("InvalidMethodName"));
            }
            if (result == null) {
                throw new NullReferenceException
                    ("Couldn't invoke object as create SmsService.SendSms.");
            }
        }

        private static ISmsService CreateSmsService()
        {
            OrigamSettings settings = 
                ConfigurationManager.GetActiveConfiguration();
            string assembly = settings.DataDataService
                .Split(",".ToCharArray())[0].Trim();
            string classname = settings.DataDataService
                .Split(",".ToCharArray())[1].Trim();
            return Reflector.InvokeObject(
                assembly,
                classname) as ISmsService; 
        }
    }
}