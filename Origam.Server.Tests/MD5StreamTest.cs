#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using NUnit.Framework;

namespace Origam.Server.Tests
{
    [TestFixture]
    public class MD5StreamTest
    {
        [Test]
        public void CalculateHash()
        {
        //    String inputValue = "huhu";
        //    MemoryStream input = new MemoryStream(Encoding.ASCII.GetBytes(inputValue));
        //    MD5PassThroughStream transfer = new MD5PassThroughStream(input);
        //    StreamReader output = new StreamReader(transfer);
        //    String outputValue = output.ReadToEnd();
        //    byte[] md5output = transfer.GetMD5();
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < md5output.Length; i++)
        //    {
        //        sb.Append(md5output[i].ToString("x2"));
        //    }
        //    Assert.IsTrue(String.Equals(inputValue, outputValue), "Input and output values are different.");
        //    Assert.IsTrue(String.Equals(sb.ToString(), "f3c2cefc1f3b082a56f52902484ca511"), "Hash doesn't match.");
        }

    }
}
