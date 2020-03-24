#region license

/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;

namespace Origam.ServerCore.Configuration
{
    public class LanguageConfig
    {
        public RequestCulture Default { get; }
        public CultureInfo[] Allowed { get; }
        
        public LanguageConfig(IConfiguration configuration)
        {
            IConfigurationSection languageSection = configuration
                .GetSection("LanguageConfig");
            string defaultCulture = languageSection
                .GetValue<string>("Default");
            Default = new RequestCulture(defaultCulture);

            string[] allowedCultures = languageSection
                .GetSection("Allowed").Get<string[]>() 
                                       ?? new []{ defaultCulture};
            Allowed = allowedCultures 
                .Select(x => new CultureInfo(x))
                .ToArray();
        }
    }
}