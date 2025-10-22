#region license
/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

using System.Globalization;
using System.IO;
using Origam.JSON;
using Origam.Service.Core;
using Origam.Workflow;

namespace Origam.Workflow;

public class XmlJsonConvertServiceAgent : AbstractServiceAgent
{
    private object result;
    public override object Result => result;

    public override void Run()
    {
        switch (this.MethodName)
        {
            case "TypedXml2Json":
            {
                StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                JsonUtils.SerializeToJson(
                    textWriter: stringWriter,
                    value: Parameters.Get<IDataDocument>("Data").DataSet,
                    omitRootElement: Parameters.TryGet<bool>("OmitRootElement"),
                    omitMainElement: Parameters.TryGet<bool>("OmitMainElement")
                );
                result = stringWriter.ToString();
                break;
            }
        }
    }
}
