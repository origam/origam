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
using Origam.Extensions;

namespace Origam.ServerCore.Configuration
{
    public class LanguageConfig
    {
        public RequestCulture DefaultCulture { get; }
        public CultureInfo[] AllowedCultures { get; }
        public CultureItem[] CultureItems { get; }

        public LanguageConfig(IConfiguration configuration)
        {
            IConfigurationSection languageSection = configuration
                .GetSection("LanguageConfig");
            string defaultCulture = languageSection
                .GetValue<string>("Default");
            DefaultCulture = new RequestCulture(defaultCulture);
            
            CultureItems = languageSection
                .GetSection("Allowed")
                .GetChildren()
                .Select(section => 
                    new CultureItem
                    {
                        CultureName = section.GetValue<string>("Culture"),
                        Caption = section.GetValue<string>("Caption"),
                    })
                .ToArray();

            AllowedCultures = CultureItems
                .Select(item => new CultureInfo(item.CultureName))
                .ToArray();
        }
    }

    public class CultureItem
    {
        public string CultureName { get; set; }
        public string Caption { get; set; }
    }
}