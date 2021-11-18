// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// Resource.cs
//
// Wrapper to make the use of resources easy on the code.
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
using System.Globalization;
using System.IO;
using System.Resources;
using System.Reflection; 

namespace Microsoft.ApplicationBlocks.ConfigurationManagement
{
    /// <summary>
    /// Helper class used to manage application resources
    /// </summary>
    internal sealed class Resource
    {
        #region Static part

        private static readonly Resource _resource;

		//  static constructor private by nature.  Initialize our read-only member _resourceManager here, 
		//  there will only ever be one copy.
		static Resource()
		{
			_resource = new Resource();
		}


		//  return the singleton instance of Resource
        public static Resource ResourceManager
        {
            get
            {
                return _resource;
            }
        }
        
		
		#endregion
		
        #region Instance part
        
		//  this is the ACTUAL resource manager, for which this class is just a convenience wrapper
		private ResourceManager _resourceManager = null;

		//  make constructor private so no one can directly create an instance of Resource, only use the Static Property ResourceManager
        private Resource()
        {
            _resourceManager = new ResourceManager(this.GetType().Namespace + ".ConfigurationManagerText", Assembly.GetExecutingAssembly());
        }


		//  a convenience Indexer that access the internal resource manager
        public string this [ string key ]
        {
            get
            {
                return _resourceManager.GetString( key, CultureInfo.CurrentCulture );
            }
        }


		public string this [ string key, params object[] par ]
		{
			get
			{
				return string.Format( CultureInfo.CurrentUICulture, _resourceManager.GetString( key, CultureInfo.CurrentCulture ), par );
			}
		}

		public Stream GetStream( string name )
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream( name );
		}
		#endregion
    }
}
