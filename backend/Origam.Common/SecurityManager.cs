#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Origam;

public static class SecurityManager
{
    public const string ROLE_SUFFIX_DIVIDER = "|";
    public const string READ_ONLY_ROLE_SUFFIX = "isReadOnly()";
    public const string INITIAL_SCREEN_ROLE_SUFFIX = "isInitialScreen()";
    public const string BUILTIN_SUPER_USER_ROLE = "e0ad1a0b-3e05-4b97-be38-12ff63e7f2f2";
    private static IOrigamProfileProvider _profileProvider = null;
    private static IOrigamAuthorizationProvider _authorizationProvider = null;
    private static IServiceProvider _DIServiceProvider = null;

    public static void SetDIServiceProvider(IServiceProvider diServiceProvider)
    {
        _DIServiceProvider = diServiceProvider;
    }

    public static IServiceProvider DIServiceProvider => _DIServiceProvider;

    public static IOrigamAuthorizationProvider GetAuthorizationProvider()
    {
        if (_authorizationProvider == null)
        {
            string[] providerSplit = ConfigurationManager
                .GetActiveConfiguration()
                .AuthorizationProvider.Split(',');
            string assembly = providerSplit[0].Trim();
            string className = providerSplit[1].Trim();
            _authorizationProvider = (IOrigamAuthorizationProvider)
                Reflector.InvokeObject(assembly, className);
        }
        return _authorizationProvider;
    }

    public static IOrigamProfileProvider GetProfileProvider()
    {
        if (_profileProvider == null)
        {
            string[] providerSplit = ConfigurationManager
                .GetActiveConfiguration()
                .ProfileProvider.Split(',');
            string assembly = providerSplit[0].Trim();
            string className = providerSplit[1].Trim();
            _profileProvider = (IOrigamProfileProvider)Reflector.InvokeObject(assembly, className);
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
        return AddRoleSuffix(roles, READ_ONLY_ROLE_SUFFIX);
    }

    public static string GetInitialScreenRoles(string roles)
    {
        return AddRoleSuffix(roles, INITIAL_SCREEN_ROLE_SUFFIX);
    }

    private static string AddRoleSuffix(string roles, string suffix)
    {
        string authContext = "";
        if (roles != null)
        {
            string[] roleList = roles.Split(";".ToCharArray());
            foreach (string role in roleList)
            {
                authContext += role + ROLE_SUFFIX_DIVIDER + suffix + ";";
            }
        }
        return authContext;
    }

    public static void SetServerIdentity()
    {
        Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("origam_server"), null);
    }

    public static void SetCustomIdentity(string userName, HttpContext context)
    {
        var principal = new GenericPrincipal(new GenericIdentity(userName), null);
        Thread.CurrentPrincipal = principal;
        context.User = new ClaimsPrincipal(principal);
    }

    public static IPrincipal CurrentPrincipal
    {
        get
        {
            IPrincipal principal = null;
            // if there is a IPrincipal service in the DI, use it first.
            try
            {
                principal = _DIServiceProvider?.GetService<IPrincipal>();
            }
            catch (ObjectDisposedException)
            {
                // request not coming from controller,
                // but still in server environment
            }
            if (principal != null)
            {
                return principal;
            }
            // fallback to the old approach
            principal = Thread.CurrentPrincipal;
            if (principal == null)
            {
                throw new UserNotLoggedInException();
            }
            return principal;
        }
    }

    public static UserProfile CurrentUserProfile()
    {
        IOrigamProfileProvider profileProvider = GetProfileProvider();
        return (UserProfile)profileProvider.GetProfile(CurrentPrincipal.Identity);
    }
}
