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

namespace Origam.Schema;
using System;
using System.Collections;


/// <summary>
///     <para>
///       A collection that stores <see cref='Origam.Schema.SchemaItemAncestor'/> objects.
///    </para>
/// </summary>
/// <seealso cref='Origam.Schema.SchemaItemAncestorCollection'/>
[Serializable()]
public class SchemaItemAncestorCollection : CollectionBase {
    
    /// <summary>
    ///     <para>
    ///       Initializes a new instance of <see cref='Origam.Schema.SchemaItemAncestorCollection'/>.
    ///    </para>
    /// </summary>
    public SchemaItemAncestorCollection() {
    }
    
    /// <summary>
    ///     <para>
    ///       Initializes a new instance of <see cref='Origam.Schema.SchemaItemAncestorCollection'/> based on another <see cref='Origam.Schema.SchemaItemAncestorCollection'/>.
    ///    </para>
    /// </summary>
    /// <param name='value'>
    ///       A <see cref='Origam.Schema.SchemaItemAncestorCollection'/> from which the contents are copied
    /// </param>
    public SchemaItemAncestorCollection(SchemaItemAncestorCollection value) {
        this.AddRange(value);
    }
    
    /// <summary>
    ///     <para>
    ///       Initializes a new instance of <see cref='Origam.Schema.SchemaItemAncestorCollection'/> containing any array of <see cref='Origam.Schema.SchemaItemAncestor'/> objects.
    ///    </para>
    /// </summary>
    /// <param name='value'>
    ///       A array of <see cref='Origam.Schema.SchemaItemAncestor'/> objects with which to intialize the collection
    /// </param>
    public SchemaItemAncestorCollection(SchemaItemAncestor[] value) {
        this.AddRange(value);
    }
    
    /// <summary>
    /// <para>Represents the entry at the specified index of the <see cref='Origam.Schema.SchemaItemAncestor'/>.</para>
    /// </summary>
    /// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
    /// <value>
    ///    <para> The entry at the specified index of the collection.</para>
    /// </value>
    /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
    public SchemaItemAncestor this[int index] {
        get {
            return ((SchemaItemAncestor)(List[index]));
        }
        set {
            List[index] = value;
        }
    }
    
    /// <summary>
    ///    <para>Adds a <see cref='Origam.Schema.SchemaItemAncestor'/> with the specified value to the 
    ///    <see cref='Origam.Schema.SchemaItemAncestorCollection'/> .</para>
    /// </summary>
    /// <param name='value'>The <see cref='Origam.Schema.SchemaItemAncestor'/> to add.</param>
    /// <returns>
    ///    <para>The index at which the new element was inserted.</para>
    /// </returns>
    /// <seealso cref='Origam.Schema.SchemaItemAncestorCollection.AddRange'/>
    public int Add(SchemaItemAncestor value) {
        return List.Add(value);
    }
    
    /// <summary>
    /// <para>Copies the elements of an array to the end of the <see cref='Origam.Schema.SchemaItemAncestorCollection'/>.</para>
    /// </summary>
    /// <param name='value'>
    ///    An array of type <see cref='Origam.Schema.SchemaItemAncestor'/> containing the objects to add to the collection.
    /// </param>
    /// <returns>
    ///   <para>None.</para>
    /// </returns>
    /// <seealso cref='Origam.Schema.SchemaItemAncestorCollection.Add'/>
    public void AddRange(SchemaItemAncestor[] value) {
        for (int i = 0; (i < value.Length); i = (i + 1)) {
            this.Add(value[i]);
        }
    }
    
    /// <summary>
    ///     <para>
    ///       Adds the contents of another <see cref='Origam.Schema.SchemaItemAncestorCollection'/> to the end of the collection.
    ///    </para>
    /// </summary>
    /// <param name='value'>
    ///    A <see cref='Origam.Schema.SchemaItemAncestorCollection'/> containing the objects to add to the collection.
    /// </param>
    /// <returns>
    ///   <para>None.</para>
    /// </returns>
    /// <seealso cref='Origam.Schema.SchemaItemAncestorCollection.Add'/>
    public void AddRange(SchemaItemAncestorCollection value) {
        for (int i = 0; (i < value.Count); i = (i + 1)) {
            this.Add(value[i]);
        }
    }
    
    /// <summary>
    /// <para>Gets a value indicating whether the 
    ///    <see cref='Origam.Schema.SchemaItemAncestorCollection'/> contains the specified <see cref='Origam.Schema.SchemaItemAncestor'/>.</para>
    /// </summary>
    /// <param name='value'>The <see cref='Origam.Schema.SchemaItemAncestor'/> to locate.</param>
    /// <returns>
    /// <para><see langword='true'/> if the <see cref='Origam.Schema.SchemaItemAncestor'/> is contained in the collection; 
    ///   otherwise, <see langword='false'/>.</para>
    /// </returns>
    /// <seealso cref='Origam.Schema.SchemaItemAncestorCollection.IndexOf'/>
    public bool Contains(SchemaItemAncestor item) {
		if (item == null)
		{
			for (int i = 0; i < this.Count; i++)
			{
				if (this[i] == null)
				{
					return true;
				}
			}
			return false;
		}
		for (int j = 0; j < this.Count; j++)
		{
			if (item.Ancestor != null && item.Ancestor.PrimaryKey.Equals((this[j] as SchemaItemAncestor).Ancestor.PrimaryKey))
			{
				return true;
			}
		}
		return false;
    }
    
    /// <summary>
    /// <para>Copies the <see cref='Origam.Schema.SchemaItemAncestorCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
    ///    specified index.</para>
    /// </summary>
    /// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='Origam.Schema.SchemaItemAncestorCollection'/> .</para></param>
    /// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
    /// <returns>
    ///   <para>None.</para>
    /// </returns>
    /// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='Origam.Schema.SchemaItemAncestorCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
    /// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
    /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
    /// <seealso cref='System.Array'/>
    public void CopyTo(SchemaItemAncestor[] array, int index) {
        List.CopyTo(array, index);
    }
    
    /// <summary>
    ///    <para>Returns the index of a <see cref='Origam.Schema.SchemaItemAncestor'/> in 
    ///       the <see cref='Origam.Schema.SchemaItemAncestorCollection'/> .</para>
    /// </summary>
    /// <param name='value'>The <see cref='Origam.Schema.SchemaItemAncestor'/> to locate.</param>
    /// <returns>
    /// <para>The index of the <see cref='Origam.Schema.SchemaItemAncestor'/> of <paramref name='value'/> in the 
    /// <see cref='Origam.Schema.SchemaItemAncestorCollection'/>, if found; otherwise, -1.</para>
    /// </returns>
    /// <seealso cref='Origam.Schema.SchemaItemAncestorCollection.Contains'/>
    public int IndexOf(SchemaItemAncestor value) {
        return List.IndexOf(value);
    }
    
    /// <summary>
    /// <para>Inserts a <see cref='Origam.Schema.SchemaItemAncestor'/> into the <see cref='Origam.Schema.SchemaItemAncestorCollection'/> at the specified index.</para>
    /// </summary>
    /// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
    /// <param name=' value'>The <see cref='Origam.Schema.SchemaItemAncestor'/> to insert.</param>
    /// <returns><para>None.</para></returns>
    /// <seealso cref='Origam.Schema.SchemaItemAncestorCollection.Add'/>
    public void Insert(int index, SchemaItemAncestor value) {
        List.Insert(index, value);
    }
    
    /// <summary>
    ///    <para>Returns an enumerator that can iterate through 
    ///       the <see cref='Origam.Schema.SchemaItemAncestorCollection'/> .</para>
    /// </summary>
    /// <returns><para>None.</para></returns>
    /// <seealso cref='System.Collections.IEnumerator'/>
    public new SchemaItemAncestorEnumerator GetEnumerator() {
        return new SchemaItemAncestorEnumerator(this);
    }
    
    /// <summary>
    ///    <para> Removes a specific <see cref='Origam.Schema.SchemaItemAncestor'/> from the 
    ///    <see cref='Origam.Schema.SchemaItemAncestorCollection'/> .</para>
    /// </summary>
    /// <param name='value'>The <see cref='Origam.Schema.SchemaItemAncestor'/> to remove from the <see cref='Origam.Schema.SchemaItemAncestorCollection'/> .</param>
    /// <returns><para>None.</para></returns>
    /// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
    public void Remove(SchemaItemAncestor value) {
        List.Remove(value);
    }
    
	public override string ToString()
	{
		string result = "";
		foreach(SchemaItemAncestor item in this)
		{
			if(result != "") result += ", ";
			if(item.Ancestor == null)
			{
				result += "!!! Unspecified !!!";
			}
			else
			{
				result += item.Ancestor.Name;
			}
		}
		return result;
	}
    public class SchemaItemAncestorEnumerator : object, IEnumerator 
	{
        
        private IEnumerator baseEnumerator;
        
        private IEnumerable temp;
        
        public SchemaItemAncestorEnumerator(SchemaItemAncestorCollection mappings) {
            this.temp = ((IEnumerable)(mappings));
            this.baseEnumerator = temp.GetEnumerator();
        }
        
        public SchemaItemAncestor Current {
            get {
                return ((SchemaItemAncestor)(baseEnumerator.Current));
            }
        }
        
        object IEnumerator.Current {
            get {
                return baseEnumerator.Current;
            }
        }
        
        public bool MoveNext() {
            return baseEnumerator.MoveNext();
        }
        
        bool IEnumerator.MoveNext() {
            return baseEnumerator.MoveNext();
        }
        
        public void Reset() {
            baseEnumerator.Reset();
        }
        
        void IEnumerator.Reset() {
            baseEnumerator.Reset();
        }
    }
}
