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
﻿//
// Copyright © 2012 - 2013 Nauck IT KG     http://www.nauck-it.de
//
// Author:
//  Daniel Nauck        <d.nauck(at)nauck-it.de>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.ComponentModel;

namespace Origam.Licensing
{
	/// <summary>
	/// Interface that is used to build fluent interfaces and hides methods declared by <see cref="object"/> from IntelliSense.
	/// </summary>
	/// <remarks>
	/// Code that consumes implementations of this interface should expect one of two things:
	/// <list type = "number">
	///   <item>When referencing the interface from within the same solution (project reference), you will still see the methods this interface is meant to hide.</item>
	///   <item>When referencing the interface through the compiled output assembly (external reference), the standard Object methods will be hidden as intended.</item>
	/// </list>
	/// See http://bit.ly/ifluentinterface for more information.
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IFluentInterface
	{
		/// <summary>
		/// Redeclaration that hides the <see cref="object.GetType()"/> method from IntelliSense.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		Type GetType();

		/// <summary>
		/// Redeclaration that hides the <see cref="object.GetHashCode()"/> method from IntelliSense.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		int GetHashCode();

		/// <summary>
		/// Redeclaration that hides the <see cref="object.ToString()"/> method from IntelliSense.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		string ToString();

		/// <summary>
		/// Redeclaration that hides the <see cref="object.Equals(object)"/> method from IntelliSense.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		bool Equals(object obj);
	}
}