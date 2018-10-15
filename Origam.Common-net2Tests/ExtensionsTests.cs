using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Origam.Extensions;
using Origam.Schema;
using MoreLinq;
using NUnit.Framework;

namespace Origam.Common_net2Tests
{
    [TestFixture]
    public class DictiaoaryExtensionsTests
    {
        [Test]
        public void ShouldRemoveValues()
        {
            int distSize = 1000000;
            int itemsToKeep = 10000;

            
            var testDictionary = GenerateRandomDictionary(distSize);

            HashSet<ReferenceTypeInt> valuesToKeep = testDictionary.Values
                .Take(itemsToKeep)
                .ToHashSet();   

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
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
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
            IEnumerable<Type> allPublicSubTypes = typeof(AbstractSchemaItem).GetAllPublicSubTypes().ToList();
        }
    }
    
    [TestFixture]
    public class DirectoryInfoExtensionTests
    {
        [Test]
        public void ShouldRecognizeDirectoryAsParent()
        {
            var parent = new DirectoryInfo(@"C:\Bordel\Serialization\Root Menu");
            var child = new DirectoryInfo(@"C:\Bordel\Serialization\Root Menu\DeploymentVersion\Root Menu");

            Assert.That(parent.IsOnPathOf(child));
        }
        
        [Test]
        public void ShouldRecognizeDirectoryIsNotParent()
        {
            var notApatent = new DirectoryInfo(@"C:\Bordel\Serialization\Root");
            var child = new DirectoryInfo(@"C:\Bordel\Serialization\Root Menu\DeploymentVersion\Root Menu");

            Assert.That(!notApatent.IsOnPathOf(child));
        }
    }

}