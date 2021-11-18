// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// DataProtection.cs
//
// Data protection implementation that uses DPAPI for encryption and MACHASH for
// hashing.
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
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.Security.Permissions;
using System.Security.Cryptography;
using Microsoft.Win32;

using Microsoft.ApplicationBlocks.ConfigurationManagement;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement.DataProtection
{
	/// <summary>
	/// The key store used for the DPAPI encryption
	/// </summary>
    public enum Store { 
		/// <summary>
		/// Use the machine key to encrypt the data
		/// </summary>
		Machine = 1, 
		
		/// <summary>
		/// Use the user key to encrypt the data
		/// </summary>
		User 
	};

	/// <summary>
	/// The DPAPI wrapper
	/// </summary>
	internal class DPAPIDataProtection : IDataProtection
	{
		#region Constants
		private const int					CRYPTPROTECT_UI_FORBIDDEN		= 0x1;
		private const int					CRYPTPROTECT_LOCAL_MACHINE		= 0x4;
		private								Store store;
		#endregion

		#region P/Invoke structures
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		internal struct DATA_BLOB
		{
			public int cbData;
			public IntPtr pbData;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)] 
		internal struct CRYPTPROTECT_PROMPTSTRUCT
		{
			public int cbSize;
			public int dwPromptFlags;
			public IntPtr hwndApp;
			public String szPrompt;
		}
		#endregion

		#region External methods
		[DllImport("Crypt32.dll", SetLastError=true, CharSet=CharSet.Auto)] 
		private static extern bool CryptProtectData(
			ref DATA_BLOB pDataIn, 
			String szDataDescr, 
			ref DATA_BLOB pOptionalEntropy,
			IntPtr pvReserved, 
			ref CRYPTPROTECT_PROMPTSTRUCT 
			pPromptStruct, 
			int dwFlags, 
			ref DATA_BLOB pDataOut);

		[DllImport("Crypt32.dll", SetLastError=true, CharSet=CharSet.Auto)]
		private static extern bool CryptUnprotectData(
			ref DATA_BLOB pDataIn, 
			String szDataDescr, 
			ref DATA_BLOB pOptionalEntropy, 
			IntPtr pvReserved, 
			ref CRYPTPROTECT_PROMPTSTRUCT 
			pPromptStruct, 
			int dwFlags, 
			ref DATA_BLOB pDataOut);
		
		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		private static extern int FormatMessage(
			int dwFlags, 
			ref IntPtr lpSource, 
			int dwMessageId,
			int dwLanguageId, 
			ref String lpBuffer, 
			int nSize,
			IntPtr Arguments);

		#endregion

		#region Constructor
        public DPAPIDataProtection() : this ( Store.Machine ){}
 
        public DPAPIDataProtection( Store tempStore )
		{
		    store = tempStore;
		}
		#endregion

		#region Declare members
		byte[] _macKey = null;
		#endregion

		#region IDataProtection implementation
		
		public void Init( ListDictionary initParams ) 
		{
			SecurityPermission sp = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
			sp.Assert();

            string keyStoreString = initParams[ "keyStore" ] as string;
            if( keyStoreString != null && keyStoreString.Length != 0 )
                store = (Store)Enum.Parse( typeof(Store), keyStoreString, true ); 
            else store = Store.Machine;

			string base64Key = null;
			string regKey = initParams[ "hashKeyRegistryPath" ] as string;
			if( regKey != null && regKey.Length != 0 )
				base64Key = DataProtectionHelper.GetRegistryDefaultValue( regKey, "hashKey", "hashKeyRegistryPath" );

			if( base64Key == null || base64Key.Length == 0 )
				base64Key = initParams[ "hashKey" ] as string;

			if( base64Key == null || base64Key.Length == 0 )
				throw new Exception( Resource.ResourceManager[ "Res_ExceptionEmptyHashKey" ] );

			// Get the hashkey bytes from the base64 string
			_macKey = Convert.FromBase64String( base64Key );
		}
		
		/// <summary>
		/// Encrypt the given data
		/// </summary>
		public byte[] Encrypt( byte[] plainText )
		{
			SecurityPermission sp = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
			sp.Assert();

            return Encrypt( plainText, null );
		}

		public byte[] Decrypt( byte[] cipherText )
		{
			SecurityPermission sp = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
			sp.Assert();

			return Decrypt( cipherText, null );
		}
		
		byte[] IDataProtection.ComputeHash( byte[] hashedData )
		{
			// Compute the hash
			HMACSHA1 mac = new HMACSHA1( _macKey );
			return mac.ComputeHash( hashedData, 0, hashedData.Length );
		}

		#endregion
		
		#region Private methods

		byte[] Encrypt(byte[] plainText, byte[] optionalEntropy)
		{
			DATA_BLOB plainTextBlob = new DATA_BLOB();
			DATA_BLOB cipherTextBlob = new DATA_BLOB();
			DATA_BLOB entropyBlob = new DATA_BLOB();

			CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT();
			InitPromptstruct(ref prompt);

			int dwFlags;
			try
			{
				try
				{
					int bytesSize = plainText.Length;
					plainTextBlob.pbData = Marshal.AllocHGlobal(bytesSize);
					if(IntPtr.Zero.Equals( plainTextBlob.pbData ))
					{
						throw new Exception( Resource.ResourceManager[ "Res_UnableToAllocateBuffer" ] );
					}
					plainTextBlob.cbData = bytesSize;
					Marshal.Copy(plainText, 0, plainTextBlob.pbData, bytesSize);
				}
				catch(Exception ex)
				{
					throw new Exception( Resource.ResourceManager[ "Res_ExceptionMarshallingData" ], ex);
				}
				if(Store.Machine == store)
				{
					// Using the machine store, should be providing entropy.
					dwFlags = CRYPTPROTECT_LOCAL_MACHINE|CRYPTPROTECT_UI_FORBIDDEN;
					// Check to see if the entropy is null
					if(optionalEntropy==null)
					{
						// Allocate something
						optionalEntropy = new byte[0];
					}
					try
					{
						int bytesSize = optionalEntropy.Length;
						entropyBlob.pbData = Marshal.AllocHGlobal(optionalEntropy.Length);
						if(IntPtr.Zero == entropyBlob.pbData)
						{
							throw new Exception( Resource.ResourceManager[ "Res_ExceptionAllocatingEntropyBuffer" ] );
						}
						Marshal.Copy(optionalEntropy, 0, entropyBlob.pbData, bytesSize);
						entropyBlob.cbData = bytesSize;
					}
					catch(Exception ex)
					{
						throw new Exception( Resource.ResourceManager[ "Res_ExceptionEntropyMarshallingData" ], ex);
					}
				}
				else
				{
					// Using the user store
					dwFlags = CRYPTPROTECT_UI_FORBIDDEN;
				}
				if( !CryptProtectData( ref plainTextBlob, "", ref entropyBlob, IntPtr.Zero, ref prompt, dwFlags, ref cipherTextBlob) )
				{
					throw new Exception( Resource.ResourceManager[ "Res_ExceptionEncryptionFailed" ] + GetErrorMessage(Marshal.GetLastWin32Error() ) );
				}
			}
			catch(Exception ex)
			{
				throw new Exception( Resource.ResourceManager[ "Res_ExceptionEncryptionFailed" ] + ex.Message, ex);
			}
			finally
			{
				if( plainText != null )
					Array.Clear( plainText, 0, plainText.Length );
				if( optionalEntropy != null )
					Array.Clear( optionalEntropy, 0, optionalEntropy.Length );
			}

			byte[] cipherText = new byte[cipherTextBlob.cbData];
			Marshal.Copy(cipherTextBlob.pbData, cipherText, 0, cipherTextBlob.cbData);
			return cipherText;
		}


		byte[] Decrypt(byte[] cipherText, byte[] optionalEntropy)
		{
            DATA_BLOB plainTextBlob = new DATA_BLOB();
			DATA_BLOB cipherBlob = new DATA_BLOB();
			CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT();
			InitPromptstruct(ref prompt);
			try
			{
				try
				{
					int cipherTextSize = cipherText.Length;
					cipherBlob.pbData = Marshal.AllocHGlobal(cipherTextSize);
					if(IntPtr.Zero == cipherBlob.pbData)
					{
						throw new Exception( Resource.ResourceManager[ "Res_ExceptionUnableToAllecateCipherTextBuffer" ] );
					}
					cipherBlob.cbData = cipherTextSize;
					Marshal.Copy(cipherText, 0, cipherBlob.pbData, cipherBlob.cbData);
				}
				catch(Exception ex)
				{
					throw new Exception( Resource.ResourceManager[ "Res_ExceptionMarshallingData" ], ex);
				}
				DATA_BLOB entropyBlob = new DATA_BLOB();
				int dwFlags;
				if(Store.Machine == store)
				{
					// Using the machine store, should be providing entropy.
					dwFlags = CRYPTPROTECT_LOCAL_MACHINE|CRYPTPROTECT_UI_FORBIDDEN;
					// Check to see if the entropy is null
					if(null == optionalEntropy)
					{
						// Allocate something
						optionalEntropy = new byte[0];
					}
					try
					{
						int bytesSize = optionalEntropy.Length;
						entropyBlob.pbData = Marshal.AllocHGlobal(bytesSize);
						if(IntPtr.Zero == entropyBlob.pbData)
						{
							throw new Exception( Resource.ResourceManager[ "Res_ExceptionAllocatingEntropyBuffer" ] );
						}
						entropyBlob.cbData = bytesSize;
						Marshal.Copy(optionalEntropy, 0, entropyBlob.pbData, bytesSize);
					}
					catch(Exception ex)
					{
						throw new Exception( Resource.ResourceManager[ "Res_ExceptionEntropyMarshallingData" ], ex);
					}
				}
				else
				{
					// Using the user store
					dwFlags = CRYPTPROTECT_UI_FORBIDDEN;
				}
				if( !CryptUnprotectData(ref cipherBlob, null, ref entropyBlob, IntPtr.Zero, ref prompt, dwFlags, ref plainTextBlob) )
				{
					throw new Exception( Resource.ResourceManager[ "Res_ExceptionDecryptionFailed" ] + GetErrorMessage(Marshal.GetLastWin32Error()));
				}
				// Free the blob and entropy.
				if(IntPtr.Zero != cipherBlob.pbData)
				{
					Marshal.FreeHGlobal(cipherBlob.pbData);
				}
				if(IntPtr.Zero != entropyBlob.pbData)
				{
					Marshal.FreeHGlobal(entropyBlob.pbData);
				}
			}
			catch(Exception ex)
			{
				throw new Exception( Resource.ResourceManager[ "Res_ExceptionDecryptionFailed" ] + ex.Message, ex);
			}
			byte[] plainText = new byte[plainTextBlob.cbData];
			Marshal.Copy(plainTextBlob.pbData, plainText, 0, plainTextBlob.cbData);
			return plainText;
		}
				
		private void InitPromptstruct(ref CRYPTPROTECT_PROMPTSTRUCT ps) 
		{
			ps.cbSize = Marshal.SizeOf(typeof(CRYPTPROTECT_PROMPTSTRUCT));
			ps.dwPromptFlags = 0;
			ps.hwndApp = IntPtr.Zero;
			ps.szPrompt = null;
		}

		private static String GetErrorMessage(int errorCode)
		{
			int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
			int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
			int FORMAT_MESSAGE_FROM_SYSTEM  = 0x00001000;
			int messageSize = 255;
			String lpMsgBuf = "";
			int dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER | 
				FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;
			IntPtr ptrlpSource = new IntPtr();
			IntPtr prtArguments = new IntPtr();
			int retVal = FormatMessage(dwFlags, ref ptrlpSource, errorCode, 0, 
				ref lpMsgBuf, messageSize, IntPtr.Zero );
			if(0 == retVal)
			{
				throw new Exception( Resource.ResourceManager[ "Res_ExceptionFormattingMessage", errorCode ] );
			}
			return lpMsgBuf;
		}
		#endregion

		#region IDisposable implementation
		/// <summary>
		/// Close the unmanaged resources
		/// </summary>
		void IDisposable.Dispose()
		{
			// Not used, no unmanaged members
		}
		
		#endregion
	}
}

namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{
	internal class DataProtectionHelper
	{
		public static string GetRegistryDefaultValue( string regKey, string valueName, string attributeName )
		{
            RegistryKey baseKey = null;
			try
			{
				if( regKey.ToUpper( System.Globalization.CultureInfo.CurrentUICulture ).StartsWith( Registry.LocalMachine.Name ) )
					baseKey = Registry.LocalMachine;
				else if( regKey.ToUpper( System.Globalization.CultureInfo.CurrentUICulture ).StartsWith( Registry.CurrentUser.Name ) )
					baseKey = Registry.CurrentUser;
				else if( regKey.ToUpper( System.Globalization.CultureInfo.CurrentUICulture ).StartsWith( Registry.Users.Name ) )
					baseKey = Registry.Users;
				else
					throw new Exception( Resource.ResourceManager[ "Res_ExceptionInvalidRegKeyFormat", regKey, attributeName ] );
				
				int idxFirstSlash = regKey.IndexOf( "\\" )+1;
				string keyPath = regKey.Substring( idxFirstSlash, regKey.Length - idxFirstSlash  );
				
				using( RegistryKey valueKey = baseKey.OpenSubKey( keyPath, false ) )
				{
					if( valueKey == null )
						throw new Exception( Resource.ResourceManager[ "Res_ExceptionInvalidRegKeyFormatCantFoundKey", regKey, attributeName ] );
					return valueKey.GetValue( valueName, "" ) as string; 
				}
			}
			finally
			{
				if( baseKey != null ) 
					((IDisposable)baseKey).Dispose();
			}
		}
	}
}
