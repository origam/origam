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
using System.ComponentModel;
using Origam.Schema.GuiModel;
using System.Text;
using System.ComponentModel.Design;
using System.Drawing.Design; 

namespace Origam.Gui.Win;

/// <summary>
///     <para>
///       A collection that stores <see cref='Origam.Gui.Win.ColumnParameterMapping'/> objects.
///    </para>
/// </summary>
/// <seealso cref='Origam.Gui.Win.ColumnParameterMappingCollection'/>
[Serializable()]
[Editor(typeof(ColumnParameterMappingCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
public class ColumnParameterMappingCollection : CollectionBase, ICustomTypeDescriptor {
        
	/// <summary>
	///     <para>
	///       Initializes a new instance of <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/>.
	///    </para>
	/// </summary>
	public ColumnParameterMappingCollection() {
        }
        
	/// <summary>
	///     <para>
	///       Initializes a new instance of <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> based on another <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/>.
	///    </para>
	/// </summary>
	/// <param name='value'>
	///       A <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> from which the contents are copied
	/// </param>
	public ColumnParameterMappingCollection(ColumnParameterMappingCollection value) {
            this.AddRange(value);
        }
        
	/// <summary>
	///     <para>
	///       Initializes a new instance of <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> containing any array of <see cref='Origam.Gui.Win.ColumnParameterMapping'/> objects.
	///    </para>
	/// </summary>
	/// <param name='value'>
	///       A array of <see cref='Origam.Gui.Win.ColumnParameterMapping'/> objects with which to intialize the collection
	/// </param>
	public ColumnParameterMappingCollection(ColumnParameterMapping[] value) {
            this.AddRange(value);
        }
        
	/// <summary>
	/// <para>Represents the entry at the specified index of the <see cref='Origam.Gui.Win.ColumnParameterMapping'/>.</para>
	/// </summary>
	/// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
	/// <value>
	///    <para> The entry at the specified index of the collection.</para>
	/// </value>
	/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
	public ColumnParameterMapping this[int index] {
		get {
                return ((ColumnParameterMapping)(List[index]));
            }
		set {
                List[index] = value;
            }
	}
        
	/// <summary>
	///    <para>Adds a <see cref='Origam.Gui.Win.ColumnParameterMapping'/> with the specified value to the 
	///    <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> .</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Gui.Win.ColumnParameterMapping'/> to add.</param>
	/// <returns>
	///    <para>The index at which the new element was inserted.</para>
	/// </returns>
	/// <seealso cref='Origam.Gui.Win.ColumnParameterMappingCollection.AddRange'/>
	public int Add(ColumnParameterMapping value) {
            return List.Add(value);
        }
        
	/// <summary>
	/// <para>Copies the elements of an array to the end of the <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/>.</para>
	/// </summary>
	/// <param name='value'>
	///    An array of type <see cref='Origam.Gui.Win.ColumnParameterMapping'/> containing the objects to add to the collection.
	/// </param>
	/// <returns>
	///   <para>None.</para>
	/// </returns>
	/// <seealso cref='Origam.Gui.Win.ColumnParameterMappingCollection.Add'/>
	public void AddRange(ColumnParameterMapping[] value) {
            for (int i = 0; (i < value.Length); i = (i + 1)) {
                this.Add(value[i]);
            }
        }
        
	/// <summary>
	///     <para>
	///       Adds the contents of another <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> to the end of the collection.
	///    </para>
	/// </summary>
	/// <param name='value'>
	///    A <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> containing the objects to add to the collection.
	/// </param>
	/// <returns>
	///   <para>None.</para>
	/// </returns>
	/// <seealso cref='Origam.Gui.Win.ColumnParameterMappingCollection.Add'/>
	public void AddRange(ColumnParameterMappingCollection value) {
            for (int i = 0; (i < value.Count); i = (i + 1)) {
                this.Add(value[i]);
            }
        }
        
	/// <summary>
	/// <para>Gets a value indicating whether the 
	///    <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> contains the specified <see cref='Origam.Gui.Win.ColumnParameterMapping'/>.</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Gui.Win.ColumnParameterMapping'/> to locate.</param>
	/// <returns>
	/// <para><see langword='true'/> if the <see cref='Origam.Gui.Win.ColumnParameterMapping'/> is contained in the collection; 
	///   otherwise, <see langword='false'/>.</para>
	/// </returns>
	/// <seealso cref='Origam.Gui.Win.ColumnParameterMappingCollection.IndexOf'/>
	public bool Contains(ColumnParameterMapping value) {
            return List.Contains(value);
        }
        
	/// <summary>
	/// <para>Copies the <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
	///    specified index.</para>
	/// </summary>
	/// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> .</para></param>
	/// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
	/// <returns>
	///   <para>None.</para>
	/// </returns>
	/// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
	/// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
	/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
	/// <seealso cref='System.Array'/>
	public void CopyTo(ColumnParameterMapping[] array, int index) {
            List.CopyTo(array, index);
        }
        
	/// <summary>
	///    <para>Returns the index of a <see cref='Origam.Gui.Win.ColumnParameterMapping'/> in 
	///       the <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> .</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Gui.Win.ColumnParameterMapping'/> to locate.</param>
	/// <returns>
	/// <para>The index of the <see cref='Origam.Gui.Win.ColumnParameterMapping'/> of <paramref name='value'/> in the 
	/// <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/>, if found; otherwise, -1.</para>
	/// </returns>
	/// <seealso cref='Origam.Gui.Win.ColumnParameterMappingCollection.Contains'/>
	public int IndexOf(ColumnParameterMapping value) {
            return List.IndexOf(value);
        }
        
	/// <summary>
	/// <para>Inserts a <see cref='Origam.Gui.Win.ColumnParameterMapping'/> into the <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> at the specified index.</para>
	/// </summary>
	/// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
	/// <param name=' value'>The <see cref='Origam.Gui.Win.ColumnParameterMapping'/> to insert.</param>
	/// <returns><para>None.</para></returns>
	/// <seealso cref='Origam.Gui.Win.ColumnParameterMappingCollection.Add'/>
	public void Insert(int index, ColumnParameterMapping value) {
            List.Insert(index, value);
        }
        
	/// <summary>
	///    <para>Returns an enumerator that can iterate through 
	///       the <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> .</para>
	/// </summary>
	/// <returns><para>None.</para></returns>
	/// <seealso cref='System.Collections.IEnumerator'/>
	public new ColumnParameterMappingEnumerator GetEnumerator() {
            return new ColumnParameterMappingEnumerator(this);
        }
        
	/// <summary>
	///    <para> Removes a specific <see cref='Origam.Gui.Win.ColumnParameterMapping'/> from the 
	///    <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> .</para>
	/// </summary>
	/// <param name='value'>The <see cref='Origam.Gui.Win.ColumnParameterMapping'/> to remove from the <see cref='Origam.Gui.Win.ColumnParameterMappingCollection'/> .</param>
	/// <returns><para>None.</para></returns>
	/// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
	public void Remove(ColumnParameterMapping value) {
            List.Remove(value);
        }
        
	public class ColumnParameterMappingEnumerator : object, IEnumerator {
            
		private IEnumerator baseEnumerator;
            
		private IEnumerable temp;
            
		public ColumnParameterMappingEnumerator(ColumnParameterMappingCollection mappings) {
                this.temp = ((IEnumerable)(mappings));
                this.baseEnumerator = temp.GetEnumerator();
            }
            
		public ColumnParameterMapping Current {
			get {
                    return ((ColumnParameterMapping)(baseEnumerator.Current));
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

	#region ICustomTypeDescriptor implementation
	public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
			return GetProperties();
		}

	public PropertyDescriptorCollection GetProperties() 
	{
			// Create a new collection object PropertyDescriptorCollection
			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

			// Iterate the list of items
			for( int i = 0; i < this.List.Count; i++ )
			{
				// For each item create a property descriptor 		// and add it to the 		// PropertyDescriptorCollection instance
				ColumnParameterMappingPropertyDescriptor pd = new 
					ColumnParameterMappingPropertyDescriptor(this, i);
				pds.Add(pd);
			}
			return pds;
		}

	public String GetClassName()
	{
			return TypeDescriptor.GetClassName(this,true);
		}

	public AttributeCollection GetAttributes()
	{
			return TypeDescriptor.GetAttributes(this,true);
		}

	public String GetComponentName()
	{
			return TypeDescriptor.GetComponentName(this, true);
		}

	public TypeConverter GetConverter()
	{
			return TypeDescriptor.GetConverter(this, true);
		}

	public EventDescriptor GetDefaultEvent() 
	{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

	public PropertyDescriptor GetDefaultProperty() 
	{
			return TypeDescriptor.GetDefaultProperty(this, true);
		}

	public object GetEditor(Type editorBaseType) 
	{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

	public EventDescriptorCollection GetEvents(Attribute[] attributes) 
	{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

	public EventDescriptorCollection GetEvents()
	{
			return TypeDescriptor.GetEvents(this, true);
		}

	public object GetPropertyOwner(PropertyDescriptor pd) 
	{
			return this;
		}
	#endregion
}

public class ColumnParameterMappingPropertyDescriptor : PropertyDescriptor
{
	private ColumnParameterMappingCollection collection = null;
	private int index = -1;

	public ColumnParameterMappingPropertyDescriptor(ColumnParameterMappingCollection coll, 
		int idx) : base( "#"+idx.ToString(), null )
	{
			this.collection = coll;
			this.index = idx;
		} 

	public override AttributeCollection Attributes
	{
		get 
		{ 
				return new AttributeCollection(null);
			}
	}

	public override bool CanResetValue(object component)
	{
			return true;
		}

	public override Type ComponentType
	{
		get 
		{ 
				return this.collection.GetType();
			}
	}

	public override string DisplayName
	{
		get 
		{
				if (this.collection.Count <= index) {
					return null;
				}

				ColumnParameterMapping mapping = this.collection[index];
				return mapping.Name;
			}
	}

	public override string Description
	{
		get
		{
				ColumnParameterMapping mapping = this.collection[index];
				StringBuilder sb = new StringBuilder();
				sb.Append(mapping.Name);
				sb.Append(",");
				sb.Append(mapping.ColumnName);

				return sb.ToString();
			}
	}

	public override object GetValue(object component)
	{
			ColumnParameterMapping mapping = this.collection[index] as ColumnParameterMapping;
			return mapping.ColumnName;
		}

	public override bool IsReadOnly
	{
		get { return false;  }
	}

	public override string Name
	{
		get { return "#"+index.ToString(); }
	}

	public override Type PropertyType
	{
		get { return typeof(string); }
	}

	public override void ResetValue(object component) {}

	public override bool ShouldSerializeValue(object component)
	{
			return true;
		}

	public override void SetValue(object component, object value)
	{
			ColumnParameterMapping mapping = this.collection[index] as ColumnParameterMapping;
			mapping.ColumnName = (string)value;
		}
}

public class ColumnParameterMappingCollectionConverter : ExpandableObjectConverter
{
	public override object ConvertTo(ITypeDescriptorContext context, 
		System.Globalization.CultureInfo culture, 
		object value, Type destinationType)
	{
			ColumnParameterMappingCollection col = value as ColumnParameterMappingCollection;
			if(destinationType == typeof(string) && col != null)
			{
				int count = col.Count;
				if (count == 1) {
					return "1 parameter";
				} else {
					return col.Count.ToString() + " parameters";
				}
			}
			return base.ConvertTo(context,culture,value,destinationType);
		}
}

public class ColumnParameterMappingCollectionEditor : CollectionEditor
{
	public ColumnParameterMappingCollectionEditor(Type type)
		: base(type)
	{
		}

	/// <summary>
	/// This is the way to remove the collection editor button from the property grid.
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
			return UITypeEditorEditStyle.None;
		}
}
