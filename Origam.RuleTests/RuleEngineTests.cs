using System;
using NUnit.Framework;


namespace Origam.Rule.Tests
{
    [TestFixture]
    class RuleEngineTests
    {
        [TestCase("0000000000{",2, 0.0)]
        [TestCase("5545A",2,554.51)] 
        [TestCase("45a",0,451)] 
        [TestCase("10}",2,-1.00)]
        [TestCase("45D",3,0.454)]
        [TestCase("45d",0, 454.0)]
        [TestCase("21M",1, -21.4)]
        [TestCase("21}",0, -210.0)]
        [TestCase("21{",0, 210.0)]
        public void ShouldDecodeSignedOverpunch(string signedOverpunchVal,
            int decimalPlaces,double expectedNumber)
        {
            var decodedNum = RuleEngine.DecodeSignedOverpunch(
                signedOverpunchVal,decimalPlaces);

            Assert.That(
                decodedNum,Is.EqualTo(expectedNumber).Within(0.000000000000001));
        }

        
        [TestCase("21+",1)]
        [TestCase("21535",2)]
        [TestCase("dvdb",1)]
        [TestCase("dvdbA",1)]
        [TestCase("",0)]
        [TestCase("1",0)]
        [TestCase(null,1)]
        [TestCase("45D",5)]
        public void ShouldFailToDecodeSignedOverpunch(string invalidOverpunchVal,
            int decimalPlaces)
        {
            Assert.Throws<ArgumentException>(
                () => RuleEngine.DecodeSignedOverpunch(
                    invalidOverpunchVal,
                    decimalPlaces)
                );
        }
    }
}