#region license

/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using System.Collections.Generic;
using NUnit.Framework;
using Origam.Schema;

namespace Origam.SchemaTests;

[TestFixture]
public class ParameterReferenceTests
{
    [Test]
    public void RefreshPreservesParameterNamesForExistingReaders()
    {
        var item = new ParameterizedSchemaItem();
        var previousKeys = item.ParameterReferences.Keys;

        item.ParameterName = "UpdatedParameter";
        item.ClearCache();
        var currentKeys = item.ParameterReferences.Keys;

        Assert.That(previousKeys, Is.EquivalentTo(new[] { "OriginalParameter" }));
        Assert.That(currentKeys, Is.EquivalentTo(new[] { "UpdatedParameter" }));
    }

    private sealed class ParameterizedSchemaItem : AbstractSchemaItem
    {
        public override string ItemType => "ParameterizedSchemaItem";
        public string ParameterName { get; set; } = "OriginalParameter";

        public override void GetParameterReferences(
            ISchemaItem parentItem,
            Dictionary<string, ParameterReference> list
        )
        {
            list.Add(ParameterName, null);
        }
    }
}
