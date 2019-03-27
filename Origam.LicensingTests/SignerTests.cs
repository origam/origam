#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Security.Cryptography;

namespace Origam.Licensing.Tests
{
	[TestClass()]
	public class SignerTests
	{
		[TestMethod()]
		public void TestAll()
		{
			try
			{
				// Create a UnicodeEncoder to convert between byte array and string.
				ASCIIEncoding ByteConverter = new ASCIIEncoding();

				string dataString = "Data to Sign";

				// Create byte arrays to hold original, encrypted, and decrypted data.
				byte[] originalData = ByteConverter.GetBytes(dataString);
				byte[] signedData;

				// Create a new instance of the RSACryptoServiceProvider class 
				// and automatically create a new key-pair.
				//RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider(2048);
				RSACryptoServiceProvider RSAalg = Signer.getKeyFromTheContainer();

				// Export the key information to an RSAParameters object.
				// You must pass true to export the private key for signing.
				// However, you do not need to export the private key
				// for verification.
				RSAParameters key = RSAalg.ExportParameters(true);
				//RSAParameters publicKey = RSAalg.ExportParameters(false);

				//string s = RSAalg.ToXmlString(true);

				// Hash and sign the data.
				signedData = Signer.HashAndSignBytes(originalData, key);

				// Verify the data and display the result to the 
				// console.
				if (Signer.VerifySignedHash(originalData, signedData))
				{
					Console.WriteLine("The data was verified.");
				}
				else
				{
					Console.WriteLine("The data does not match the signature.");
					Assert.Fail();
				}

			}
			catch (ArgumentNullException)
			{
				Console.WriteLine("The data was not signed or verified");
				Assert.Fail();
			}
		}

		[TestMethod()]
		public void StorePrivateKeyInContainer()
		{
			//RSACSPSample.storePrivateKeyToMachineContainer("xxx");
		}

		[TestMethod()]
		public void DeletePrivateKeyInContainer()
		{
			Signer.DeleteKeyFromContainer();
		}
	}

}