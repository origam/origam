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
using System.IO;
using System.Linq;
using NUnit.Framework;
using Origam.Extensions;
using Origam.Schema;

namespace Origam.Common_net2Tests;

[TestFixture]
public class DictionaryExtensionsTests
{
    [Test]
    public void ShouldRemoveValues()
    {
        int distSize = 1000000;
        int itemsToKeep = 10000;

        var testDictionary = GenerateRandomDictionary(numOfEntries: distSize);
        HashSet<ReferenceTypeInt> valuesToKeep = new HashSet<ReferenceTypeInt>(
            collection: testDictionary.Values.Take(count: itemsToKeep)
        );
        testDictionary.RemoveByValueSelector(valueSelectorFunc: value =>
            !valuesToKeep.Contains(item: value)
        );

        Assert.That(actual: testDictionary, expression: Has.Count.EqualTo(expected: itemsToKeep));
    }

    private Dictionary<int, ReferenceTypeInt> GenerateRandomDictionary(int numOfEntries)
    {
        return Enumerable
            .Range(start: 1, count: numOfEntries)
            .ToDictionary(keySelector: x => x, elementSelector: x => new ReferenceTypeInt(val: x));
    }

    class ReferenceTypeInt
    {
        private readonly int val;

        public ReferenceTypeInt(int val)
        {
            this.val = val;
        }

        protected bool Equals(ReferenceTypeInt other) => val == other.val;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(objA: null, objB: obj))
            {
                return false;
            }

            if (ReferenceEquals(objA: this, objB: obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals(other: (ReferenceTypeInt)obj);
        }

        public override int GetHashCode() => val;
    }
}

[TestFixture]
public class TypeExtensionsTests
{
    [Test]
    public void ShouldRemoveValues()
    {
        IEnumerable<Type> allPublicSubTypes = typeof(ISchemaItem).GetAllPublicSubTypes().ToList();
    }
}

[TestFixture]
public class DirectoryInfoExtensionTests
{
    [Test]
    public void ShouldRecognizeDirectoryAsParent()
    {
        var parent = new DirectoryInfo(
            path: @"Serialization\Root Menu".Replace(
                oldChar: '\\',
                newChar: Path.DirectorySeparatorChar
            )
        );
        var child = new DirectoryInfo(
            path: @"Serialization\Root Menu\DeploymentVersion\Root Menu".Replace(
                oldChar: '\\',
                newChar: Path.DirectorySeparatorChar
            )
        );
        Assert.That(condition: parent.IsOnPathOf(other: child));
    }

    [Test]
    public void ShouldRecognizeDirectoryIsNotParent()
    {
        var notApatent = new DirectoryInfo(
            path: @"Serialization\Root".Replace(oldChar: '\\', newChar: Path.DirectorySeparatorChar)
        );
        var child = new DirectoryInfo(
            path: @"Serialization\Root Menu\DeploymentVersion\Root Menu".Replace(
                oldChar: '\\',
                newChar: Path.DirectorySeparatorChar
            )
        );
        Assert.That(condition: !notApatent.IsOnPathOf(other: child));
    }
}

[TestFixture]
public class StringExtensionTests
{
    [Test]
    public void ShouldTruncateString()
    {
        string stringTestValue = "The quick brown fox jumps over the lazy dog.";
        string nullString = null;
        Assert.That(condition: stringTestValue.Truncate(maxLength: 0).Equals(value: string.Empty));
        Assert.That(condition: stringTestValue.Truncate(maxLength: 9).Equals(value: "The quick"));
        Assert.That(
            condition: stringTestValue.Truncate(maxLength: 100).Equals(value: stringTestValue)
        );
        Assert.That(
            condition: stringTestValue.Truncate(maxLength: -10).Equals(value: string.Empty)
        );
        Assert.That(condition: nullString.Truncate(maxLength: 0) is null);
        Assert.That(condition: nullString.Truncate(maxLength: 10) is null);
    }
}
