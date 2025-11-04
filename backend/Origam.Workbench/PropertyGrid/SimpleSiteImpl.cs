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
using System.Collections.Generic;
using System.ComponentModel;

namespace Origam.Workbench.PropertyGrid;

public sealed class SimpleSiteImpl : ISite, IServiceProvider
{
    public IComponent Component { get; set; }
    private readonly IContainer container = new Container();
    IContainer ISite.Container
    {
        get { return container; }
    }
    public bool DesignMode { get; set; }
    public string Name { get; set; }
    private Dictionary<Type, object> services;

    public void AddService<T>(T service)
        where T : class
    {
        if (services == null)
        {
            services = new Dictionary<Type, object>();
        }

        services[typeof(T)] = service;
    }

    public void RemoveService<T>()
        where T : class
    {
        if (services != null)
        {
            services.Remove(typeof(T));
        }
    }

    object IServiceProvider.GetService(Type serviceType)
    {
        object service;
        if (services != null && services.TryGetValue(serviceType, out service))
        {
            return service;
        }
        return null;
    }
}
