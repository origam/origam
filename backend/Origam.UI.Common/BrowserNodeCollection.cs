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

namespace Origam.UI {
    using System;
    using System.Collections;
    
    
    /// <summary>
    ///     <para>
    ///       A collection that stores <see cref='Origam.UI.IBrowserNode'/> objects.
    ///    </para>
    /// </summary>
    /// <seealso cref='Origam.UI.BrowserNodeCollection'/>
    [Serializable()]
    public class BrowserNodeCollection : CollectionBase {
        
        /// <summary>
        ///     <para>
        ///       Initializes a new instance of <see cref='Origam.UI.BrowserNodeCollection'/>.
        ///    </para>
        /// </summary>
        public BrowserNodeCollection() {
        }
        
        /// <summary>
        ///     <para>
        ///       Initializes a new instance of <see cref='Origam.UI.BrowserNodeCollection'/> based on another <see cref='Origam.UI.BrowserNodeCollection'/>.
        ///    </para>
        /// </summary>
        /// <param name='value'>
        ///       A <see cref='Origam.UI.BrowserNodeCollection'/> from which the contents are copied
        /// </param>
        public BrowserNodeCollection(BrowserNodeCollection value) {
            this.AddRange(value);
        }
        
        /// <summary>
        ///     <para>
        ///       Initializes a new instance of <see cref='Origam.UI.BrowserNodeCollection'/> containing any array of <see cref='Origam.UI.IBrowserNode'/> objects.
        ///    </para>
        /// </summary>
        /// <param name='value'>
        ///       A array of <see cref='Origam.UI.IBrowserNode'/> objects with which to intialize the collection
        /// </param>
        public BrowserNodeCollection(IBrowserNode[] value) {
            this.AddRange(value);
        }
        
        /// <summary>
        /// <para>Represents the entry at the specified index of the <see cref='Origam.UI.IBrowserNode'/>.</para>
        /// </summary>
        /// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
        /// <value>
        ///    <para> The entry at the specified index of the collection.</para>
        /// </value>
        /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
        public IBrowserNode this[int index] {
            get {
                return ((IBrowserNode)(List[index]));
            }
            set {
                List[index] = value;
            }
        }
        
        /// <summary>
        ///    <para>Adds a <see cref='Origam.UI.IBrowserNode'/> with the specified value to the 
        ///    <see cref='Origam.UI.BrowserNodeCollection'/> .</para>
        /// </summary>
        /// <param name='value'>The <see cref='Origam.UI.IBrowserNode'/> to add.</param>
        /// <returns>
        ///    <para>The index at which the new element was inserted.</para>
        /// </returns>
        /// <seealso cref='Origam.UI.BrowserNodeCollection.AddRange'/>
        public int Add(IBrowserNode value) {
            return List.Add(value);
        }
        
        /// <summary>
        /// <para>Copies the elements of an array to the end of the <see cref='Origam.UI.BrowserNodeCollection'/>.</para>
        /// </summary>
        /// <param name='value'>
        ///    An array of type <see cref='Origam.UI.IBrowserNode'/> containing the objects to add to the collection.
        /// </param>
        /// <returns>
        ///   <para>None.</para>
        /// </returns>
        /// <seealso cref='Origam.UI.BrowserNodeCollection.Add'/>
        public void AddRange(IBrowserNode[] value) {
            for (int i = 0; (i < value.Length); i = (i + 1)) {
                this.Add(value[i]);
            }
        }
        
        /// <summary>
        ///     <para>
        ///       Adds the contents of another <see cref='Origam.UI.BrowserNodeCollection'/> to the end of the collection.
        ///    </para>
        /// </summary>
        /// <param name='value'>
        ///    A <see cref='Origam.UI.BrowserNodeCollection'/> containing the objects to add to the collection.
        /// </param>
        /// <returns>
        ///   <para>None.</para>
        /// </returns>
        /// <seealso cref='Origam.UI.BrowserNodeCollection.Add'/>
        public void AddRange(BrowserNodeCollection value) {
            for (int i = 0; (i < value.Count); i = (i + 1)) {
                this.Add(value[i]);
            }
        }
        
        /// <summary>
        /// <para>Gets a value indicating whether the 
        ///    <see cref='Origam.UI.BrowserNodeCollection'/> contains the specified <see cref='Origam.UI.IBrowserNode'/>.</para>
        /// </summary>
        /// <param name='value'>The <see cref='Origam.UI.IBrowserNode'/> to locate.</param>
        /// <returns>
        /// <para><see langword='true'/> if the <see cref='Origam.UI.IBrowserNode'/> is contained in the collection; 
        ///   otherwise, <see langword='false'/>.</para>
        /// </returns>
        /// <seealso cref='Origam.UI.BrowserNodeCollection.IndexOf'/>
        public bool Contains(IBrowserNode value) {
            return List.Contains(value);
        }
        
        /// <summary>
        /// <para>Copies the <see cref='Origam.UI.BrowserNodeCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
        ///    specified index.</para>
        /// </summary>
        /// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='Origam.UI.BrowserNodeCollection'/> .</para></param>
        /// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
        /// <returns>
        ///   <para>None.</para>
        /// </returns>
        /// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='Origam.UI.BrowserNodeCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
        /// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
        /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
        /// <seealso cref='System.Array'/>
        public void CopyTo(IBrowserNode[] array, int index) {
            List.CopyTo(array, index);
        }
        
        /// <summary>
        ///    <para>Returns the index of a <see cref='Origam.UI.IBrowserNode'/> in 
        ///       the <see cref='Origam.UI.BrowserNodeCollection'/> .</para>
        /// </summary>
        /// <param name='value'>The <see cref='Origam.UI.IBrowserNode'/> to locate.</param>
        /// <returns>
        /// <para>The index of the <see cref='Origam.UI.IBrowserNode'/> of <paramref name='value'/> in the 
        /// <see cref='Origam.UI.BrowserNodeCollection'/>, if found; otherwise, -1.</para>
        /// </returns>
        /// <seealso cref='Origam.UI.BrowserNodeCollection.Contains'/>
        public int IndexOf(IBrowserNode value) {
            return List.IndexOf(value);
        }
        
        /// <summary>
        /// <para>Inserts a <see cref='Origam.UI.IBrowserNode'/> into the <see cref='Origam.UI.BrowserNodeCollection'/> at the specified index.</para>
        /// </summary>
        /// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
        /// <param name=' value'>The <see cref='Origam.UI.IBrowserNode'/> to insert.</param>
        /// <returns><para>None.</para></returns>
        /// <seealso cref='Origam.UI.BrowserNodeCollection.Add'/>
        public void Insert(int index, IBrowserNode value) {
            List.Insert(index, value);
        }
        
        /// <summary>
        ///    <para>Returns an enumerator that can iterate through 
        ///       the <see cref='Origam.UI.BrowserNodeCollection'/> .</para>
        /// </summary>
        /// <returns><para>None.</para></returns>
        /// <seealso cref='System.Collections.IEnumerator'/>
        public new IBrowserNodeEnumerator GetEnumerator() {
            return new IBrowserNodeEnumerator(this);
        }
        
        /// <summary>
        ///    <para> Removes a specific <see cref='Origam.UI.IBrowserNode'/> from the 
        ///    <see cref='Origam.UI.BrowserNodeCollection'/> .</para>
        /// </summary>
        /// <param name='value'>The <see cref='Origam.UI.IBrowserNode'/> to remove from the <see cref='Origam.UI.BrowserNodeCollection'/> .</param>
        /// <returns><para>None.</para></returns>
        /// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
        public void Remove(IBrowserNode value) {
            List.Remove(value);
        }
        
		protected override void OnSet(int index, object oldValue, object newValue) 
		{
			// TODO: Add code here to handle an existing value within
			//       the collection be replaced with a new value
		}
        
		protected override void OnInsert(int index, object value) 
		{
		}
        
		protected override void OnClear() 
		{
		}
        
		protected override void OnRemove(int index, object value) 
		{
		}
        
		protected override void OnValidate(object value) 
		{
		}

        public class IBrowserNodeEnumerator : object, IEnumerator 
		{
            
            private IEnumerator baseEnumerator;
            
            private IEnumerable temp;
            
            public IBrowserNodeEnumerator(BrowserNodeCollection mappings) {
                this.temp = ((IEnumerable)(mappings));
                this.baseEnumerator = temp.GetEnumerator();
            }
            
            public IBrowserNode Current {
                get {
                    return ((IBrowserNode)(baseEnumerator.Current));
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
}
