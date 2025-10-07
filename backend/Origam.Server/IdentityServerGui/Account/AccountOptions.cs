// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Origam.Server.IdentityServerGui.Account;

public static class AccountOptions
{
    public static bool AllowLocalLogin = true;
    public static bool ShowLogoutPrompt = false;
    public static bool AutomaticRedirectAfterSignOut = false;

    // specify the Windows authentication scheme being used
    public static readonly string WindowsAuthenticationSchemeName = Microsoft
        .AspNetCore
        .Server
        .IISIntegration
        .IISDefaults
        .AuthenticationScheme;

    // if user uses windows auth, should we load the groups from windows
    public static bool IncludeWindowsGroups = false;
}
