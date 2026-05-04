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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;

namespace Origam.Gui.Designer;

/// <summary>
/// Summary description for SiteImpl.
/// </summary>
public class SiteImpl : ISite, IDictionaryService
{
    private IComponent component;
    private string name;
    private DesignerHostImpl host;
    private DictionaryServiceImpl dictionaryService;

    public SiteImpl(IComponent comp, string name, DesignerHostImpl host)
    {
        if (comp == null)
        {
            throw new ArgumentException(message: "comp");
        }
        if (host == null)
        {
            throw new ArgumentException(message: "host");
        }
        if (name == null || name.Trim().Length == 0)
        {
            throw new ArgumentException(message: "name");
        }
        component = comp;
        this.host = host;
        this.name = name;
        // create a dictionary service for this site
        dictionaryService = new DictionaryServiceImpl();
    }

    #region ISite Members
    public IComponent Component
    {
        get { return component; }
    }
    public IContainer Container
    {
        get { return host.Container; }
    }
    public bool DesignMode
    {
        get { return true; }
    }
    public string Name
    {
        get { return name; }
        set
        {
            // null name is not valid
            if (value == null)
            {
                throw new ArgumentException(message: "value");
            }
            // if we have the same name
            if (string.Compare(strA: value, strB: name, ignoreCase: false) != 0)
            {
                // make sure we have a valid name
                INameCreationService nameCreationService = (INameCreationService)
                    host.GetService(serviceType: typeof(INameCreationService));
                if (nameCreationService == null)
                {
                    throw new Exception(message: "Failed to service: INameCreationService");
                }
                if (nameCreationService.IsValidName(name: value))
                {
                    DesignerHostImpl hostImpl = (DesignerHostImpl)host;
                    // get the current name
                    string oldName = name;
                    // set the new name
                    MemberDescriptor md = TypeDescriptor.CreateProperty(
                        componentType: component.GetType(),
                        name: "Name",
                        type: typeof(string),
                        attributes: new Attribute[] { }
                    );
                    // fire changing event
                    hostImpl.OnComponentChanging(component: component, member: md);
                    // set the value
                    name = value;
                    // we also have to fire the rename event
                    host.OnComponentRename(component: component, oldName: oldName, newName: name);
                    // fire changed event
                    hostImpl.OnComponentChanged(
                        component: component,
                        member: md,
                        oldValue: oldName,
                        newValue: name
                    );
                }
            }
        }
    }
    #endregion
    #region IServiceProvider Members
    public object GetService(Type service)
    {
        if (service == typeof(IDictionaryService))
        {
            return this;
        }
        // forward request to the host
        return host.GetService(serviceType: service);
    }
    #endregion

    #region IDictionaryService Implementation

    public object GetKey(object value)
    {
        return dictionaryService.GetKey(value: value);
    }

    public object GetValue(object key)
    {
        return dictionaryService.GetValue(key: key);
    }

    public void SetValue(object key, object value)
    {
        dictionaryService.SetValue(key: key, value: value);
    }

    #endregion
}
