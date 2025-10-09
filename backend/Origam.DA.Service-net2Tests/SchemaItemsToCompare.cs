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
using System.Collections.Generic;
using System.Linq;
using Origam.Schema;

namespace Origam.DA.Service_net2Tests;

internal class SchemaItemsToCompare
{
    public ISchemaItem FromDb { get; }
    public ISchemaItem FromXml { get; }

    public SchemaItemsToCompare(ISchemaItem fromDb, ISchemaItem fromXml)
    {
        FromDb = fromDb;
        FromXml = fromXml;
    }

    public override string ToString()
    {
        Dictionary<string, object> xmlDict = FromXml.GetAllProperies();
        Dictionary<string, object> dbDict = FromDb.GetAllProperies();

        return dbDict
            .Keys.Select(key => $"{key, -25}: {dbDict[key], -70} {xmlDict[key], -70}")
            .Aggregate("", (outString, str) => outString + str + Environment.NewLine);
    }

    public string Type => FromDb.GetType().ToString();
}
