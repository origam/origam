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

namespace Origam.Services
{
    /// <summary>
    /// If service implements this interface, all XslFunctions will be
    /// available in xsl transformations and xpath functions with
    /// given prefix. (If not overriden within xsl declaration)
    /// As Xpath function the defail prefix is used.
    /// </summary>
    public interface IXslFunctionProvider
    {
        // Namespace URI, something world-unique - e.g. http://schema.advantages.cz/AsapFunctions
        string NameSpaceUri { get; }
        // Prefix for xsl functions - used for XPath
        string DefaultPrefix { get; }
        // Instance of object with public functions to be exposed to XSL and XPath
        object XslFunctions { get; }
    }
}
