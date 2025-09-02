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
using System.Collections;

namespace Origam.Gui.Win;

/// <summary>
///     <para>
///       A collection that stores <see cref='Origam.Gui.Win.ComponentBinding'/> objects.
///    </para>
/// </summary>
/// <seealso cref='Origam.Gui.Win.ComponentBindingCollection'/>
[Serializable()]
public class ComponentBindingCollection : CollectionBase 
{
	/// <summary>
	///     <para>
	///       Initializes a new instance of <see cref='Origam.Gui.Win.ComponentBindingCollection'/>.
	///    </para>
	/// </summary>
	public ComponentBindingCollection() 
	{
	}
    
	/// <summary>
	///     <para>
	///       Initializes a new instance of <see cref='Origam.Gui.Win.ComponentBindingCollection'/> based on another <see cref='Origam.Gui.Win.ComponentBindingCollection'/>.
	///    </para>
	/// </summary>
	/// <param name='value'>
	///       A <see cref='Origam.Gui.Win.ComponentBindingCollection'/> from which the contents are copied
	/// </param>
	public ComponentBindingCollection(ComponentBindingCollection value) 
	{
		this.AddRange(value);
	}
    
	/// <summary>
	///     <para>
	///       Initializes a new instance of <see cref='Origam.Gui.Win.ComponentBindingCollection'/> containing any array of <see cref='Origam.Gui.Win.ComponentBinding'/> objects.
	///    </para>
	/// </summary>
	/// <param name='value'>
	///       A array of <see cref='Origam.Gui.Win.ComponentBinding'/> objects with which to intialize the collection
	/// </param>
	public ComponentBindingCollection(ComponentBinding[] value) 
	{
		this.AddRange(value);
	}
    
	/// <summary>
	/// <para>Represents the entry at the specified index of the <see cref='Origam.Gui.Win.ComponentBinding'/>.</para>
	/// </summary>
	/// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
	/// <value>
	///    <para> The entry at the specified index of the collection.</para>
	/// </value>
	/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
	public ComponentBinding this[int index] 
	{
		get 
		{
			return ((ComponentBinding)(List[index]));
		}
		set 
		{
			List[index] = value;
		}
	}
    
	/// <summary>
	///    <para>Adds a <see cref='Origam.Gui.Win.ComponentBinding'/> with the specified value to the 
	///    <see cref='Origam.Gui.Win.ComponentBindingCollection'/> .</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Gui.Win.ComponentBinding'/> to add.</param>
	/// <returns>
	///    <para>The index at which the new element was inserted.</para>
	/// </returns>
	/// <seealso cref='Origam.Gui.Win.ComponentBindingCollection.AddRange'/>
	public int Add(ComponentBinding value) 
	{
		return List.Add(value);
	}
    
	/// <summary>
	/// <para>Copies the elements of an array to the end of the <see cref='Origam.Gui.Win.ComponentBindingCollection'/>.</para>
	/// </summary>
	/// <param name='value'>
	///    An array of type <see cref='Origam.Gui.Win.ComponentBinding'/> containing the objects to add to the collection.
	/// </param>
	/// <returns>
	///   <para>None.</para>
	/// </returns>
	/// <seealso cref='Origam.Gui.Win.ComponentBindingCollection.Add'/>
	public void AddRange(ComponentBinding[] value) 
	{
		for (int i = 0; (i < value.Length); i = (i + 1)) 
		{
			this.Add(value[i]);
		}
	}
    
	/// <summary>
	///     <para>
	///       Adds the contents of another <see cref='Origam.Gui.Win.ComponentBindingCollection'/> to the end of the collection.
	///    </para>
	/// </summary>
	/// <param name='value'>
	///    A <see cref='Origam.Gui.Win.ComponentBindingCollection'/> containing the objects to add to the collection.
	/// </param>
	/// <returns>
	///   <para>None.</para>
	/// </returns>
	/// <seealso cref='Origam.Gui.Win.ComponentBindingCollection.Add'/>
	public void AddRange(ComponentBindingCollection value) 
	{
		for (int i = 0; (i < value.Count); i = (i + 1)) 
		{
			this.Add(value[i]);
		}
	}
    
	/// <summary>
	/// <para>Gets a value indicating whether the 
	///    <see cref='Origam.Gui.Win.ComponentBindingCollection'/> contains the specified <see cref='Origam.Gui.Win.ComponentBinding'/>.</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Gui.Win.ComponentBinding'/> to locate.</param>
	/// <returns>
	/// <para><see langword='true'/> if the <see cref='Origam.Gui.Win.ComponentBinding'/> is contained in the collection; 
	///   otherwise, <see langword='false'/>.</para>
	/// </returns>
	/// <seealso cref='Origam.Gui.Win.ComponentBindingCollection.IndexOf'/>
	public bool Contains(ComponentBinding value) 
	{
		return List.Contains(value);
	}
    
	/// <summary>
	/// <para>Copies the <see cref='Origam.Gui.Win.ComponentBindingCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
	///    specified index.</para>
	/// </summary>
	/// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='Origam.Gui.Win.ComponentBindingCollection'/> .</para></param>
	/// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
	/// <returns>
	///   <para>None.</para>
	/// </returns>
	/// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='Origam.Gui.Win.ComponentBindingCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
	/// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
	/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
	/// <seealso cref='System.Array'/>
	public void CopyTo(ComponentBinding[] array, int index) 
	{
		List.CopyTo(array, index);
	}
    
	/// <summary>
	///    <para>Returns the index of a <see cref='Origam.Gui.Win.ComponentBinding'/> in 
	///       the <see cref='Origam.Gui.Win.ComponentBindingCollection'/> .</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Gui.Win.ComponentBinding'/> to locate.</param>
	/// <returns>
	/// <para>The index of the <see cref='Origam.Gui.Win.ComponentBinding'/> of <paramref name='value'/> in the 
	/// <see cref='Origam.Gui.Win.ComponentBindingCollection'/>, if found; otherwise, -1.</para>
	/// </returns>
	/// <seealso cref='Origam.Gui.Win.ComponentBindingCollection.Contains'/>
	public int IndexOf(ComponentBinding value) 
	{
		return List.IndexOf(value);
	}
    
	/// <summary>
	/// <para>Inserts a <see cref='Origam.Gui.Win.ComponentBinding'/> into the <see cref='Origam.Gui.Win.ComponentBindingCollection'/> at the specified index.</para>
	/// </summary>
	/// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
	/// <param name=' value'>The <see cref='Origam.Gui.Win.ComponentBinding'/> to insert.</param>
	/// <returns><para>None.</para></returns>
	/// <seealso cref='Origam.Gui.Win.ComponentBindingCollection.Add'/>
	public void Insert(int index, ComponentBinding value) 
	{
		List.Insert(index, value);
	}
    
	/// <summary>
	///    <para>Returns an enumerator that can iterate through 
	///       the <see cref='Origam.Gui.Win.ComponentBindingCollection'/> .</para>
	/// </summary>
	/// <returns><para>None.</para></returns>
	/// <seealso cref='System.Collections.IEnumerator'/>
	public new ComponentBindingEnumerator GetEnumerator() 
	{
		return new ComponentBindingEnumerator(this);
	}
    
	/// <summary>
	///    <para> Removes a specific <see cref='Origam.Gui.Win.ComponentBinding'/> from the 
	///    <see cref='Origam.Gui.Win.ComponentBindingCollection'/> .</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Gui.Win.ComponentBinding'/> to remove from the <see cref='Origam.Gui.Win.ComponentBindingCollection'/> .</param>
	/// <returns><para>None.</para></returns>
	/// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
	public void Remove(ComponentBinding value) 
	{
		List.Remove(value);
	}
    
	public class ComponentBindingEnumerator : object, IEnumerator 
	{
		private IEnumerator baseEnumerator;
        
		private IEnumerable temp;
        
		public ComponentBindingEnumerator(ComponentBindingCollection mappings) 
		{
			this.temp = ((IEnumerable)(mappings));
			this.baseEnumerator = temp.GetEnumerator();
		}
        
		public ComponentBinding Current 
		{
			get 
			{
				return ((ComponentBinding)(baseEnumerator.Current));
			}
		}
        
		object IEnumerator.Current 
		{
			get 
			{
				return baseEnumerator.Current;
			}
		}
        
		public bool MoveNext() 
		{
			return baseEnumerator.MoveNext();
		}
        
		bool IEnumerator.MoveNext() 
		{
			return baseEnumerator.MoveNext();
		}
        
		public void Reset() 
		{
			baseEnumerator.Reset();
		}
        
		void IEnumerator.Reset() 
		{
			baseEnumerator.Reset();
		}
	}
}
