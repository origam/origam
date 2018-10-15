// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// ProductVersion.cs
//
// The product version information so all the assemblies have the same version
// and product name.
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

namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{
	/// <summary>
	/// Used to set the same version on every project on the solution
	/// </summary>
	public class Product
	{
		/// <summary>
		/// The product version
		/// </summary>
		public const string Version = "1.0.0.0";

		/// <summary>
		/// The company name
		/// </summary>
		public const string Company = "Microsoft Corp.";

		/// <summary>
		/// The project name
		/// </summary>
		public const string Name = "Configuration Management Application Block";
	}
}