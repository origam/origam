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
using Origam.Extensions;
using Origam.Schema;
using NUnit.Framework;

namespace Origam.Common_net2Tests;
[TestFixture]
public class DictionaryExtensionsTests
{
    [Test]
    public void ShouldRemoveValues()
    {
        int distSize = 1000000;
        int itemsToKeep = 10000;
        
        var testDictionary = GenerateRandomDictionary(distSize);
        HashSet<ReferenceTypeInt> valuesToKeep = new HashSet<ReferenceTypeInt>(testDictionary.Values
            .Take(itemsToKeep));   
        testDictionary
            .RemoveByValueSelector(value => !valuesToKeep.Contains(value));
        
        Assert.That(testDictionary,Has.Count.EqualTo(itemsToKeep));
    }
    private Dictionary<int, ReferenceTypeInt> GenerateRandomDictionary(
        int numOfEntries)
    {
        return Enumerable
            .Range(1, numOfEntries)
            .ToDictionary(x => x, x => new ReferenceTypeInt(x));
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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((ReferenceTypeInt) obj);
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
        var parent = new DirectoryInfo(@"Serialization\Root Menu".
            Replace('\\', Path.DirectorySeparatorChar));
        var child = new DirectoryInfo(@"Serialization\Root Menu\DeploymentVersion\Root Menu".
            Replace('\\', Path.DirectorySeparatorChar));
        Assert.That(parent.IsOnPathOf(child));
    }
    
    [Test]
    public void ShouldRecognizeDirectoryIsNotParent()
    {
        var notApatent = new DirectoryInfo(@"Serialization\Root".
            Replace('\\', Path.DirectorySeparatorChar));
        var child = new DirectoryInfo(@"Serialization\Root Menu\DeploymentVersion\Root Menu".
            Replace('\\', Path.DirectorySeparatorChar));
        Assert.That(!notApatent.IsOnPathOf(child));
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
        Assert.That(stringTestValue.Truncate(0).Equals(string.Empty));
        Assert.That(stringTestValue.Truncate(9).Equals("The quick"));
        Assert.That(stringTestValue.Truncate(100).Equals(stringTestValue));
        Assert.That(stringTestValue.Truncate(-10).Equals(string.Empty));
        Assert.That(nullString.Truncate(0) is null);
        Assert.That(nullString.Truncate(10) is null);
    }
}
