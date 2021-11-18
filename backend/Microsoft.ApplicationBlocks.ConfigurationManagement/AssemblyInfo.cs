//===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// AssemblyInfo.cs
//
// This file contains the the definitions of assembly level attributes.
//
// For more information see the Configuration Management Application Block Implementation Overview. 
// 
//===============================================================================
// Copyright (C) 2000-2001 Microsoft Corporation
// All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
//==============================================================================

using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.ApplicationBlocks.ConfigurationManagement;

[assembly: FileIOPermission(SecurityAction.RequestMinimum)]
[assembly: SqlClientPermission(SecurityAction.RequestMinimum)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, 
	Flags=SecurityPermissionFlag.UnmanagedCode | 
		  SecurityPermissionFlag.SerializationFormatter | 
		  SecurityPermissionFlag.ControlThread )]
[assembly: RegistryPermission(SecurityAction.RequestMinimum)]
[assembly: ReflectionPermission(SecurityAction.RequestMinimum, 
	Flags=ReflectionPermissionFlag.MemberAccess)]

[assembly: AssemblyTitle("")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]		

[assembly: AssemblyCompany( Product.Company )]
[assembly: AssemblyProduct( Product.Name )]
[assembly: AssemblyVersion( Product.Version )]

[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]
[assembly: AssemblyKeyName("")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]