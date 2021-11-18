// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// BCLDataProtection.cs
//
// Data protection provider sample implementation that uses base class library
// support for data protection. Uses TripleDES for encryption and MACHASH for hashing.
//
// For more information see the Configuration Management Application Block Implementation Overview. 
// 
// ===============================================================================
// Copyright (C) 2000-2001 Microsoft Corporation
// All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
// ==============================================================================

using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.IO;

using Microsoft.ApplicationBlocks.ConfigurationManagement;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement.DataProtection
{
	/// <summary>
	/// Implementation of a Data Provider using the base class library cryptography support
	/// </summary>
	internal class BCLDataProtection
		: IDataProtection
	{
		#region Declare members
		private SymmetricAlgorithm _encryptionAlgorithm;
		byte[] _macKey = null;
		#endregion

		/// <summary>
		/// Default constructor
		/// </summary>
		public BCLDataProtection() { }

		#region Implementation of IDataProtection

		/// <summary>
		/// The initialization method used to get defaults from the configuration file
		/// </summary>
		/// <param name="dataProtectionParameters"></param>
		public void Init( ListDictionary dataProtectionParameters )
		{
			// Create an instance of the Triple-DES crypto 
			_encryptionAlgorithm = TripleDESCryptoServiceProvider.Create();

			// Process the configuration parameters
			string base64Key = null;
			string regKey = dataProtectionParameters[ "hashKeyRegistryPath" ] as string;
			if( regKey != null && regKey.Length != 0 )
				base64Key = DataProtectionHelper.GetRegistryDefaultValue( regKey, "hashKey", "hashKeyRegistryPath" );

			if( base64Key == null || base64Key.Length == 0 )
				base64Key = dataProtectionParameters[ "hashKey" ] as string;

			if( base64Key == null || base64Key.Length == 0 )
				throw new Exception( Resource.ResourceManager[ "Res_ExceptionEmptyHashKey" ] );

			// Get the key bytes from the base64 string
			_macKey = Convert.FromBase64String( base64Key );

			base64Key = null;
			regKey = dataProtectionParameters[ "symmetricKeyRegistryPath" ] as string;
			if( regKey != null && regKey.Length != 0 )
				base64Key = DataProtectionHelper.GetRegistryDefaultValue( regKey, "symmetricKey", "symmetricKeyRegistryPath" );

			if( base64Key == null || base64Key.Length == 0 )
				base64Key = dataProtectionParameters[ "symmetricKey" ] as string;

			if( base64Key == null || base64Key.Length == 0 )
				throw new Exception( Resource.ResourceManager[ "Res_ExceptionEmptySymmetricKey" ] );

			// Set the key
			_encryptionAlgorithm.Key = Convert.FromBase64String( base64Key );

			base64Key = null;
			regKey = dataProtectionParameters[ "initializationVectorRegistryPath" ] as string;
			if( regKey != null && regKey.Length != 0 )
				base64Key = DataProtectionHelper.GetRegistryDefaultValue( regKey, "initializationVector", "initializationVectorRegistryPath" );

			if( base64Key == null || base64Key.Length == 0 )
				base64Key = dataProtectionParameters[ "initializationVector" ] as string;

			if( base64Key == null || base64Key.Length == 0 )
				throw new Exception( Resource.ResourceManager[ "Res_ExceptionEmptyInitializationVectorKey" ] );

			// Set the IV
			_encryptionAlgorithm.IV = Convert.FromBase64String( base64Key );
		}

		/// <summary>
		/// Encryption method
		/// </summary>
		public byte[] Encrypt( byte[] plainText )
		{
			byte[] cipherValue = null;
			byte[] plainValue = plainText;    

			MemoryStream memStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream( memStream, _encryptionAlgorithm.CreateEncryptor(), CryptoStreamMode.Write );
			try
			{
				// Write the encrypted information
				cryptoStream.Write( plainValue, 0, plainValue.Length );
				cryptoStream.Flush(); 
				cryptoStream.FlushFinalBlock(); 
    
				// Get the encrypted stream
				cipherValue = memStream.ToArray();
			}
			catch( Exception e )
			{
				throw new ApplicationException( Resource.ResourceManager[ "Res_ExceptionCantEncrypt" ], e );
			}
			finally
			{
				// Clear the arrays
				if( plainValue != null )
					Array.Clear( plainValue, 0, plainValue.Length );
				memStream.Close();
				cryptoStream.Close();
			}

			return cipherValue;
		}

		/// <summary>
		/// Decryption method
		/// </summary>
		public byte[] Decrypt( byte[] cipherText )
		{
			byte[] cipherValue = cipherText; 
			byte[] plainValue = new byte[ cipherValue.Length ];

			MemoryStream memStream = new MemoryStream(cipherValue);
			CryptoStream cryptoStream = new CryptoStream( memStream, _encryptionAlgorithm.CreateDecryptor(), CryptoStreamMode.Read );
			try
			{
				// Decrypt the data
				cryptoStream.Read( plainValue, 0, plainValue.Length );
			}
			catch( Exception e )
			{
				throw new ApplicationException( Resource.ResourceManager[ "Res_ExceptionCantDecrypt" ], e );
			}
			finally
			{
				// Clear the arrays
				if( cipherValue != null )
					Array.Clear( cipherValue, 0, cipherValue.Length );
				memStream.Close();
				// Flush the stream buffer
				cryptoStream.Close();
			}
			return plainValue ;
		}

		/// <summary>
		/// Compute a hash for an arbitrary string
		/// </summary>
		/// <param name="plainText">The plain text to hash</param>
		/// <returns>The hash to compute</returns>
		public byte[] ComputeHash( byte[] plainText )
		{			
			// Compute the hash
			HMACSHA1 mac = new HMACSHA1( _macKey );
			byte[] hash = mac.ComputeHash( plainText, 0, plainText.Length );

			return hash ; 
		}

		#endregion

		#region IDisposable implementation
		/// <summary>
		/// Close the unmanaged resources
		/// </summary>
		public void Dispose()
		{
			_encryptionAlgorithm.Clear();
		}
		
		#endregion
	}
}
