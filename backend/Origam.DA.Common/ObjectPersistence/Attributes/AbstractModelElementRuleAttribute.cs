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

namespace Origam.DA.ObjectPersistence;

/// <summary>
/// Summary description for AbstractModelElementRuleAttribute.
/// </summary>
public abstract class AbstractModelElementRuleAttribute : Attribute, IModelElementRule
{
    private string _errorMessage;
    private string _name;

    public AbstractModelElementRuleAttribute() { }

    #region IModelElementRule Members
    public abstract Exception CheckRule(object instance);
    public abstract Exception CheckRule(object instance, string memberName);
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
    public string ErrorMessage
    {
        get { return _errorMessage; }
        set { _errorMessage = value; }
    }
    #endregion
}
