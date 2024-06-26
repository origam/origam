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

using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Origam.Server.Pages;

namespace Origam.Server.Middleware;
public class UserApiMiddleware
{
    private readonly RequestLocalizationOptions requestLocalizationOptions;
    public UserApiMiddleware(RequestDelegate next, 
        IOptions<RequestLocalizationOptions> requestLocalizationOptions)
    {
        this.requestLocalizationOptions = requestLocalizationOptions.Value;
    }
    public async Task Invoke(HttpContext context)
    {
        await SetThreadCultureFromCookie(context);
        var userApiProcessor = new CoreUserApiProcessor(new CoreHttpTools());
        var contextWrapper = new StandardHttpContextWrapper(context);
        userApiProcessor.Process(contextWrapper);
        await Task.CompletedTask;
    }
    private async Task SetThreadCultureFromCookie(HttpContext context)
    {
        var cultureProvider = requestLocalizationOptions.RequestCultureProviders
            .OfType<OrigamCookieRequestCultureProvider>().First();
        var cultureResult =
            await cultureProvider.DetermineProviderCultureResult(context);
        var culture = cultureResult?.Cultures.FirstOrDefault();
        if (culture != null)
        {
            Thread.CurrentThread.CurrentUICulture =
                new CultureInfo(culture.Value.ToString());
        }
        var uiCulture = cultureResult?.UICultures.FirstOrDefault();
        if (uiCulture != null)
        {
            Thread.CurrentThread.CurrentCulture =
                new CultureInfo(uiCulture.Value.ToString());
        }
    }
}
