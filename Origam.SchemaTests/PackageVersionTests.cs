using System;
using NUnit.Framework;
using Origam.Schema;

namespace Origam.SchemaTests
{
    [TestFixture]
    public class PackageVersionTests
    {
        [TestCase("5.0")]
        [TestCase("1.111")]
        [TestCase("1.011")]
        [TestCase("1.011.11")]
        [TestCase("")]
        [TestCase(null)]
        public void ShouldParse(string versionCandidate)
        {
            new PackageVersion(versionCandidate);
        }
        
        [TestCase("5.")]
        [TestCase("1.111A")]
        [TestCase("dcsd")]
        [TestCase("1.cdf.11")]
        public void ShouldFailToParse(string versionCandidate)
        {
            Assert.Throws<ArgumentException>(
                () => new PackageVersion(versionCandidate));
        }
        
                
        [TestCase("5.0","4.0")]
        [TestCase("5.0.1", "5.0")]
        [TestCase("5.1","5.0")]
        [TestCase("5.2","5.0.11")]
        public void ShouldRecognizeFirstIsNewer(
            string newerVersionString, string olderVersionString)
        {
            var newerVersion = new PackageVersion(newerVersionString);
            var olderVersion = new PackageVersion(olderVersionString);
            
            Assert.True(newerVersion > olderVersion);
            Assert.True(newerVersion.CompareTo(olderVersion) > 0);
        }

        [TestCase("5.0","5")]
        [TestCase("5.0","5.0")]
        [TestCase("5.0.11","5.0.011")]
        public void ShouldBerecognizedAsEqual(
            string versionString1, string versionString2)
        {
            var version1 = new PackageVersion(versionString1);
            var version2 = new PackageVersion(versionString2); 
            Assert.That(version1, Is.EqualTo(version2));
            Assert.That(version1.GetHashCode(), Is.EqualTo(version2.GetHashCode()));
        }


    }
}