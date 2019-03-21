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

using System;
using System.Security.Cryptography;

namespace Origam.Licensing
{
	public class Signer
	{
		private const string publicLicenceKey
			= "<RSAKeyValue><Modulus>teKz99NoGlLBhn4LXWgCkbHUh733Grh/unCq7ulLN8zPDcUb/GbMbW19Ejm22G2ko4vV7MIHPMnesu+Uz7ZEpXNKXZn9AXaAOwpZ4vB/wUX1RtUxRUGSkigKgWTZ4pepOZZxK0UUQgJnYIh14/0oedl8cs+vQqIHPToE1lpZ24AFsfk3k8V8VDBTrCm8U7fbKBb7QiZ4rkeHTfSG2vl1rMBqFO+wTeVUywdbHtQ0YtCYDKIlr6d8ts9AUtf4jlJTMsqTNqYPFRUQIgr1PBYUmMaC6a/5kWxpppVOQauViuQBqIdzjI08RIwTwBivJ9HZwvHQtrx6EuE1uU8apuFrwQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

		public static byte[] HashAndSignBytes(byte[] DataToSign, RSAParameters Key)
		{
			try
			{
				// Create a new instance of RSACryptoServiceProvider using the 
				// key from RSAParameters.  
				RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

				RSAalg.ImportParameters(Key);


				// Hash and sign the data. Pass a new instance of SHA1CryptoServiceProvider
				// to specify the use of SHA1 for hashing.
				return RSAalg.SignData(DataToSign, new SHA1CryptoServiceProvider());
			}
			catch (CryptographicException e)
			{
				Console.WriteLine(e.Message);

				return null;
			}
		}

		public static void storePrivateKeyToMachineContainer(string privateKeyXml)
		{
			CspParameters cspParams = new CspParameters();
			cspParams.KeyContainerName = "OrigamLicenceKey";
			cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
			cspParams.Flags = CspProviderFlags.UseArchivableKey;

			RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider(cspParams);

			//RSAalg.PersistKeyInCsp = false;

			RSAalg.FromXmlString(privateKeyXml);

			//RSAalg.ImportCspBlob(RSAalg.ExportCspBlob(true));
			//RSAalg.PersistKeyInCsp = true;
		}

		public static RSACryptoServiceProvider getKeyFromTheContainer()
		{
			CspParameters cspParams = new CspParameters();
			cspParams.KeyContainerName = "OrigamLicenceKey";
			cspParams.Flags = CspProviderFlags.UseMachineKeyStore;

			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cspParams);
			return rsa;
		}


		public static void DeleteKeyFromContainer()
		{
			// Create the CspParameters object and set the key container 
			// name used to store the RSA key pair.
			CspParameters cp = new CspParameters();
			cp.KeyContainerName = "OrigamLicenceKey";
			cp.Flags = CspProviderFlags.UseMachineKeyStore;

			// Create a new instance of RSACryptoServiceProvider that accesses
			// the key container.
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp);

			// Delete the key entry in the container.
			rsa.PersistKeyInCsp = false;

			// Call Clear to release resources and delete the key from the container.
			rsa.Clear();

			Console.WriteLine("Key deleted.");
		}

		public static bool VerifySignedHash(byte[] DataToVerify, byte[] SignedData)
		{
			try
			{
				// Create a new instance of RSACryptoServiceProvider using the 
				// key from RSAParameters.
				RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

				RSAalg.FromXmlString(publicLicenceKey);

				// Verify the data using the signature. Pass a new instance of SHA1CryptoServiceProvider
				// to specify the use of SHA1 for hashing.
				return RSAalg.VerifyData(DataToVerify, new SHA1CryptoServiceProvider(), SignedData);
			}
			catch (CryptographicException e)
			{
				Console.WriteLine(e.Message);

				return false;
			}
		}
		/*
		public static string SerializeToXml(RSAParameters key)
		{


		}
		*/
	}
	
}
