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
using System.Collections;
using Origam.Service.Core;

namespace Origam.Workflow
{
	public class HttpServiceAgent : AbstractServiceAgent
	{
		public HttpServiceAgent()
		{
		}

		#region IServiceAgent Members

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
			switch(MethodName)
			{
				case "SendRequest":
					_result = HttpTools.Instance.SendRequest(
											url: Parameters.Get<string>("Url"),
											method: Parameters.Get<string>("Method"),
											content: GetContent(Parameters["Content"]),
											contentType: Parameters.Get<string>("ContentType"),
											headers: Parameters["Headers"] as Hashtable,
											timeout: Parameters.Get<int>("Timeout")
										);
					break;
				default:
					throw new ArgumentOutOfRangeException("MethodName", MethodName, ResourceUtils.GetString("InvalidMethodName"));
			}
		}
        private string GetContent(object obj)
        {
            if (obj is XmlContainer xmlContainer)
            {
                return xmlContainer.Xml.OuterXml;
            }
            return XmlTools.ConvertToString(obj);
        }
        #endregion
    }
}
