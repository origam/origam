// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// GenericFactory.cs
//
// Factory pattern implementation, this file defines generic functionality
// for all the factories.
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
using System.Reflection;

namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{
	/// <summary>
	/// Acts as the basic implementation for the multiple Factory classes used elsewhere.
	/// We need to create instances based on config info ...
	/// Have Factories for those, and since there's much common code for doing Reflection-based activation 
	/// keep that code in one central place.
	/// 
	/// </summary>
	sealed class GenericFactory
	{

		#region Declarations

		private const string COMMA_DELIMITER	 = ",";

		#endregion

		#region Constructors

		static GenericFactory(){}
		

		private GenericFactory(){}


		#endregion

		#region Private Helper Methods

		/// <summary>
		/// Takes incoming full type string, defined as:
		/// "Microsoft.ApplicationBlocks.ConfigurationManagement.XmlFileStorage,   Microsoft.ApplicationBlocks.ConfigurationManagement, 
		///			Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
		///  And splits the type into two strings, the typeName and assemblyName.  Those are passed by as OUT params
		///  This routine also cleans up any extra whitespace, and throws an exception if the full type string
		///  does not have five comma-delimited parts....it expect the true full name complete with version and publickeytoken
		/// </summary>
		/// <param name="fullType"></param>
		/// <param name="typeName"></param>
		/// <param name="assemblyName"></param>
		private static void SplitType( string fullType, out string typeName, out string assemblyName )
		{
			string[] parts = fullType.Split( COMMA_DELIMITER.ToCharArray() );

			if ( 5 != parts.Length )
			{
				throw new ArgumentException( Resource.ResourceManager[ "RES_ExceptionBadTypeArgumentInFactory" ], "fullType" );
			}
			else
			{
				//  package type name:
				typeName = parts[0].Trim();
				//  package fully-qualified assembly name separated by commas
				assemblyName = String.Concat(   parts[1].Trim() + COMMA_DELIMITER,
												parts[2].Trim() + COMMA_DELIMITER,
												parts[3].Trim() + COMMA_DELIMITER,
												parts[4].Trim() );
				//  return
				return;
			}
		}


		#endregion

		#region Create Overloads

		/// <summary>
		/// Returns an object instantiated by the Activator, using fully-qualified combined assembly-type  supplied.
		/// "Microsoft.ApplicationBlocks.ConfigurationManagement.XmlFileStorage,   Microsoft.ApplicationBlocks.ConfigurationManagement, 
		///			Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
		/// </summary>
		/// <param name="fullTypeName">the fully-qualified type name</param>
		/// <returns>instance of requested assembly/type typed as System.Object</returns>
		public static object Create( string fullTypeName )
		{
			string assemblyName;
			string typeName;
			//  use helper to split
			SplitType( fullTypeName, out typeName, out assemblyName );
			//  just call main overload
			return Create( assemblyName, typeName, null );
		}


		/// <summary>
		/// Returns an object instantiated by the Activator, using fully-qualified asm/type supplied.
		/// Assembly parameter example: "Microsoft.ApplicationBlocks.ConfigurationManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
		/// Type parameter example: Microsoft.ApplicationBlocks.ConfigurationManagement.XmlFileStorage"
		/// </summary>
		/// <param name="assemblyName">fully-qualified assembly name</param>
		/// <param name="typeName">the type name</param>
		/// <returns>instance of requested assembly/type typed as System.Object</returns>
		public static object Create( string assemblyName, string typeName )
		{
			string aName;
			string tName;

			//  use helper to split
			SplitType( typeName + "," + assemblyName , out tName, out aName );
			
			//  just call main overload
			return Create( aName, tName, null );
		}


		/// <summary>
		/// Returns an object instantiated by the Activator, using fully-qualified asm/type supplied.
		/// Assembly parameter example: "Microsoft.ApplicationBlocks.ConfigurationManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
		/// Type parameter example: Microsoft.ApplicationBlocks.ConfigurationManagement.XmlFileStorage"
		/// FULL TYPE NAME AS WRITTEN IN CONFIG IS: 
		/// "Microsoft.ApplicationBlocks.ConfigurationManagement.XmlFileStorage,   Microsoft.ApplicationBlocks.ConfigurationManagement, 
		///			Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
		/// </summary>
		/// <param name="assemblyName">fully-qualified assembly name</param>
		/// <param name="typeName">the type name</param>
		/// <param name="constructorArguments">constructor arguments for type to be created</param>
		/// <returns>instance of requested assembly/type typed as System.Object</returns>
		public static object Create( string assemblyName, string typeName, object[] constructorArguments )
		{
			Assembly assemblyInstance			= null;
			Type typeInstance					= null;

			try 
			{
				//  use full asm name to get assembly instance
				assemblyInstance = Assembly.Load( assemblyName.Trim() );
			}
			catch ( Exception e )
			{		
				throw new TypeLoadException( 
					Resource.ResourceManager[ "RES_ExceptionCantLoadAssembly", 
					assemblyName , typeName ], 
					e );
			}
			
			try
			{
				//  use type name to get type from asm; note we WANT case specificity 
				typeInstance = assemblyInstance.GetType( typeName.Trim(), true, false );

				//  now attempt to actually create an instance, passing constructor args if available
				if( constructorArguments != null )
				{
					return Activator.CreateInstance( typeInstance, constructorArguments);
				}
				else
				{
					return Activator.CreateInstance( typeInstance );
				}
			}
			catch( Exception e )
			{	
				throw new TypeLoadException( 
					Resource.ResourceManager[ "RES_ExceptionCantCreateInstanceUsingActivate", 
					assemblyName , typeName ], 
					e );
			}
		}
		
		
		#endregion
	}
}
