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

/* All code is written my me,Ben Ratzlaff and is available for any free use except where noted.
 *I assume no responsibility for how anyone uses the information available*/

using System.Collections;

namespace Origam.Gui.Designer.PropertyGrid;

/// <summary>
/// A wrapper around a Hashtable for Setting objects. Setting objects are intended to use with the CustomPropertyGrid
/// </summary>
public class Settings
{
    private Hashtable settings;

    public Settings()
    {
        settings = new Hashtable();
    }

    /// <summary>
    /// Get the key collection for this Settings object. Every key is a string
    /// </summary>
    public ICollection Keys
    {
        get { return settings.Keys; }
    }

    /// <summary>
    /// Get/Set the Setting object tied to the input string
    /// </summary>
    public Setting this[string key]
    {
        get { return (Setting)settings[key]; }
        set
        {
            settings[key] = value;
            value.Name = key;
        }
    }
    private DesignerHostImpl _designerHost;
    public DesignerHostImpl DesignerHost
    {
        get { return _designerHost; }
        set { _designerHost = value; }
    }

    /// <summary>
    /// Gets the Setting object tied to the string. If there is no Setting object, one will be created with the defaultValue
    /// </summary>
    /// <param name="key">The name of the setting object</param>
    /// <param name="defaultvalue">if there is no Setting object tied to the string, a Setting will be created with this as its Value</param>
    /// <returns>The Setting object tied to the string</returns>
    public Setting GetSetting(string key, object defaultvalue)
    {
        if (settings[key] == null)
        {
            settings[key] = new Setting(defaultvalue, null, null);
            ((Setting)settings[key]).Name = key;
        }
        return (Setting)settings[key];
    }
}
