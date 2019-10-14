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

using System.Threading;
using System.Security.Principal;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Origam
{
	public static class SecurityManager
	{
		public const string ROLE_SUFFIX_DIVIDER = "|";
		public const string READ_ONLY_ROLE_SUFFIX = "isReadOnly()";
        public const string BUILTIN_SUPER_USER_ROLE = "e0ad1a0b-3e05-4b97-be38-12ff63e7f2f2";

		private static IOrigamProfileProvider _profileProvider = null;
		private static IOrigamAuthorizationProvider _authorizationProvider = null;

        private static IServiceProvider _DIServiceProvider = null;  
        
        public static void SetDIServiceProvider(IServiceProvider diServiceProvider)
        {
            _DIServiceProvider = diServiceProvider;
        }

        public static IOrigamAuthorizationProvider GetAuthorizationProvider()
		{
			if(_authorizationProvider == null)
			{
			    string[] providerSplit = ConfigurationManager
			        .GetActiveConfiguration()
			        .AuthorizationProvider
			        .Split(',');
			    string assembly = providerSplit[0].Trim();
			    string className = providerSplit[1].Trim();
                _authorizationProvider = 
                    (IOrigamAuthorizationProvider)Reflector.InvokeObject(assembly, className);
			}
			return _authorizationProvider;
		}
        
		public static IOrigamProfileProvider GetProfileProvider()
		{
		    if (_profileProvider == null)
		    {
		        string[] providerSplit = ConfigurationManager
		            .GetActiveConfiguration()
		            .ProfileProvider
		            .Split(',');
		        string assembly = providerSplit[0].Trim();
		        string className = providerSplit[1].Trim();
		        _profileProvider =
		            (IOrigamProfileProvider)Reflector.InvokeObject(assembly, className);
		    }
		    return _profileProvider;
        }

		public static void Reset()
		{
			_profileProvider = null;
			_authorizationProvider = null;
		}

		public static string GetReadOnlyRoles(string roles)
		{
			string authContext = "";
			if(roles != null)
			{
				string[] roleList = roles.Split(";".ToCharArray());
				foreach(string role in roleList)
				{
					authContext += role + ROLE_SUFFIX_DIVIDER + READ_ONLY_ROLE_SUFFIX + ";";
				}
			}
			return authContext;
		}

        public static void SetServerIdentity()
        {
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("origam_server"), null);
        }

        public static IPrincipal CurrentPrincipal
        {
            get
            {
                // if there is a IPrincipal service in the DI, use it first.
                IPrincipal res = _DIServiceProvider?.GetService<IPrincipal>();
                if (res != null)
                {
                    return res;
                }                    
                // fallback to the old approach
                res = Thread.CurrentPrincipal;
                if (res == null)
                {
                    throw new UserNotLoggedInException();
                }
                return res;
            }
        }

        public static UserProfile CurrentUserProfile()
        {
            IOrigamProfileProvider profileProvider = GetProfileProvider();
            return (UserProfile)profileProvider.GetProfile(CurrentPrincipal.Identity);
        }
    }
}
