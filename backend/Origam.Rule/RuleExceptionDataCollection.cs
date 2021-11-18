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

namespace Origam.Rule {
    using System;
    using System.Collections;

    /// <summary>
    ///     <para>
    ///       A collection that stores <see cref='Origam.Rule.RuleExceptionData'/> objects.
    ///    </para>
    /// </summary>
    /// <seealso cref='Origam.Rule.RuleExceptionCollection'/>
    [Serializable()]
    public class RuleExceptionDataCollection : CollectionBase {
        
        /// <summary>
        ///     <para>
        ///       Initializes a new instance of <see cref='Origam.Rule.RuleExceptionCollection'/>.
        ///    </para>
        /// </summary>
        public RuleExceptionDataCollection() {
        }
        
        /// <summary>
        ///     <para>
        ///       Initializes a new instance of <see cref='Origam.Rule.RuleExceptionCollection'/> based on another <see cref='Origam.Rule.RuleExceptionCollection'/>.
        ///    </para>
        /// </summary>
        /// <param name='value'>
        ///       A <see cref='Origam.Rule.RuleExceptionCollection'/> from which the contents are copied
        /// </param>
        public RuleExceptionDataCollection(RuleExceptionDataCollection value) {
            this.AddRange(value);
        }
        
        /// <summary>
        ///     <para>
        ///       Initializes a new instance of <see cref='Origam.Rule.RuleExceptionCollection'/> containing any array of <see cref='Origam.Rule.RuleExceptionData'/> objects.
        ///    </para>
        /// </summary>
        /// <param name='value'>
        ///       A array of <see cref='Origam.Rule.RuleExceptionData'/> objects with which to intialize the collection
        /// </param>
        public RuleExceptionDataCollection(RuleExceptionData[] value) {
            this.AddRange(value);
        }
        
        /// <summary>
        /// <para>Represents the entry at the specified index of the <see cref='Origam.Rule.RuleExceptionData'/>.</para>
        /// </summary>
        /// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
        /// <value>
        ///    <para> The entry at the specified index of the collection.</para>
        /// </value>
        /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
        public RuleExceptionData this[int index] {
            get {
                return ((RuleExceptionData)(List[index]));
            }
            set {
                List[index] = value;
            }
        }
        
        /// <summary>
        ///    <para>Adds a <see cref='Origam.Rule.RuleExceptionData'/> with the specified value to the 
        ///    <see cref='Origam.Rule.RuleExceptionCollection'/> .</para>
        /// </summary>
        /// <param name='value'>The <see cref='Origam.Rule.RuleExceptionData'/> to add.</param>
        /// <returns>
        ///    <para>The index at which the new element was inserted.</para>
        /// </returns>
        /// <seealso cref='Origam.Rule.RuleExceptionCollection.AddRange'/>
        public int Add(RuleExceptionData value) {
            return List.Add(value);
        }
        
        /// <summary>
        /// <para>Copies the elements of an array to the end of the <see cref='Origam.Rule.RuleExceptionCollection'/>.</para>
        /// </summary>
        /// <param name='value'>
        ///    An array of type <see cref='Origam.Rule.RuleExceptionData'/> containing the objects to add to the collection.
        /// </param>
        /// <returns>
        ///   <para>None.</para>
        /// </returns>
        /// <seealso cref='Origam.Rule.RuleExceptionCollection.Add'/>
        public void AddRange(RuleExceptionData[] value) {
            for (int i = 0; (i < value.Length); i = (i + 1)) {
                this.Add(value[i]);
            }
        }
        
        /// <summary>
        ///     <para>
        ///       Adds the contents of another <see cref='Origam.Rule.RuleExceptionCollection'/> to the end of the collection.
        ///    </para>
        /// </summary>
        /// <param name='value'>
        ///    A <see cref='Origam.Rule.RuleExceptionCollection'/> containing the objects to add to the collection.
        /// </param>
        /// <returns>
        ///   <para>None.</para>
        /// </returns>
        /// <seealso cref='Origam.Rule.RuleExceptionCollection.Add'/>
        public void AddRange(RuleExceptionDataCollection value) {
            for (int i = 0; (i < value.Count); i = (i + 1)) {
                this.Add(value[i]);
            }
        }
        
        /// <summary>
        /// <para>Gets a value indicating whether the 
        ///    <see cref='Origam.Rule.RuleExceptionCollection'/> contains the specified <see cref='Origam.Rule.RuleExceptionData'/>.</para>
        /// </summary>
        /// <param name='value'>The <see cref='Origam.Rule.RuleExceptionData'/> to locate.</param>
        /// <returns>
        /// <para><see langword='true'/> if the <see cref='Origam.Rule.RuleExceptionData'/> is contained in the collection; 
        ///   otherwise, <see langword='false'/>.</para>
        /// </returns>
        /// <seealso cref='Origam.Rule.RuleExceptionCollection.IndexOf'/>
        public bool Contains(RuleExceptionData value) {
            return List.Contains(value);
        }
        
        /// <summary>
        /// <para>Copies the <see cref='Origam.Rule.RuleExceptionCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
        ///    specified index.</para>
        /// </summary>
        /// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='Origam.Rule.RuleExceptionCollection'/> .</para></param>
        /// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
        /// <returns>
        ///   <para>None.</para>
        /// </returns>
        /// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='Origam.Rule.RuleExceptionCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
        /// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
        /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
        /// <seealso cref='System.Array'/>
        public void CopyTo(RuleExceptionData[] array, int index) {
            List.CopyTo(array, index);
        }
        
        /// <summary>
        ///    <para>Returns the index of a <see cref='Origam.Rule.RuleExceptionData'/> in 
        ///       the <see cref='Origam.Rule.RuleExceptionCollection'/> .</para>
        /// </summary>
        /// <param name='value'>The <see cref='Origam.Rule.RuleExceptionData'/> to locate.</param>
        /// <returns>
        /// <para>The index of the <see cref='Origam.Rule.RuleExceptionData'/> of <paramref name='value'/> in the 
        /// <see cref='Origam.Rule.RuleExceptionCollection'/>, if found; otherwise, -1.</para>
        /// </returns>
        /// <seealso cref='Origam.Rule.RuleExceptionCollection.Contains'/>
        public int IndexOf(RuleExceptionData value) {
            return List.IndexOf(value);
        }
        
        /// <summary>
        /// <para>Inserts a <see cref='Origam.Rule.RuleExceptionData'/> into the <see cref='Origam.Rule.RuleExceptionCollection'/> at the specified index.</para>
        /// </summary>
        /// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
        /// <param name=' value'>The <see cref='Origam.Rule.RuleExceptionData'/> to insert.</param>
        /// <returns><para>None.</para></returns>
        /// <seealso cref='Origam.Rule.RuleExceptionCollection.Add'/>
        public void Insert(int index, RuleExceptionData value) {
            List.Insert(index, value);
        }
        
        /// <summary>
        ///    <para>Returns an enumerator that can iterate through 
        ///       the <see cref='Origam.Rule.RuleExceptionCollection'/> .</para>
        /// </summary>
        /// <returns><para>None.</para></returns>
        /// <seealso cref='System.Collections.IEnumerator'/>
        public new RuleExceptionEnumerator GetEnumerator() {
            return new RuleExceptionEnumerator(this);
        }
        
        /// <summary>
        ///    <para> Removes a specific <see cref='Origam.Rule.RuleExceptionData'/> from the 
        ///    <see cref='Origam.Rule.RuleExceptionCollection'/> .</para>
        /// </summary>
        /// <param name='value'>The <see cref='Origam.Rule.RuleExceptionData'/> to remove from the <see cref='Origam.Rule.RuleExceptionCollection'/> .</param>
        /// <returns><para>None.</para></returns>
        /// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
        public void Remove(RuleExceptionData value) {
            List.Remove(value);
        }
        
        public class RuleExceptionEnumerator : object, IEnumerator {
            
            private IEnumerator baseEnumerator;
            
            private IEnumerable temp;
            
            public RuleExceptionEnumerator(RuleExceptionDataCollection mappings) {
                this.temp = ((IEnumerable)(mappings));
                this.baseEnumerator = temp.GetEnumerator();
            }
            
            public RuleExceptionData Current {
                get {
                    return ((RuleExceptionData)(baseEnumerator.Current));
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
