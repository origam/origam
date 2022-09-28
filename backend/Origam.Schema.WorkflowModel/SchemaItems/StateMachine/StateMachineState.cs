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

using Origam.DA.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;


namespace Origam.Schema.WorkflowModel
{
	public enum StateMachineStateType
	{
		Initial = 0,
		Running = 1,
		Final = 2,
		Group = 4
	}

	/// <summary>
	/// Summary description for StateMachineState.
	/// </summary>
	[SchemaItemDescription("State", "States", "state.png")]
    [HelpTopic("State")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class StateMachineState : AbstractSchemaItem
	{
		public const string CategoryConst = "StateMachineState";

		public StateMachineState() : base() {}

		public StateMachineState(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public StateMachineState(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members

		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		public override string Icon
		{
			get
			{
				switch(this.Type)
				{
					case StateMachineStateType.Initial:
						return "state-initial.png";
					case StateMachineStateType.Running:
						return "state-running.png";
					case StateMachineStateType.Final:
						return "state-final.png";
					case StateMachineStateType.Group:
						return "state-group.png";
					default:
						return "state.png";
				}
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			if(this.DefaultSubstate != null)	dependencies.Add(this.DefaultSubstate);

			base.GetExtraDependencies (dependencies);
		}

		public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
		{
			// move between same parent type withing the same state machine
			return ((AbstractSchemaItem)newNode).RootItem.PrimaryKey.Equals(this.RootItem.PrimaryKey)
				&& (((AbstractSchemaItem)newNode).ItemType == "WorkflowStateMachine"
					|| (((AbstractSchemaItem)newNode).ItemType == "StateMachineState")
				);
		}

		#endregion

		#region Properties
		[Browsable(false)]
		public ArrayList Operations
		{
			get
			{
				return this.ChildItemsByType(StateMachineOperation.CategoryConst);
			}
		}

		[Browsable(false)]
		public ArrayList SubStates
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach(AbstractSchemaItem item in this.ChildItemsRecursive)
				{
					if(item is StateMachineState)
					{
						result.Add(item);
					}
				}

				return result;
			}
		}

		public bool IsState(object value)
		{
			if(this.Value.Equals(value)) return true;
 
			foreach(StateMachineState state in this.SubStates)
			{
				if(state.Value.Equals(value)) return true;
			}

			return false;
		}

		private StateMachineStateType _type;

        [XmlAttribute("type")]
		public StateMachineStateType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		private int _intValue;

		[Browsable(false)]
		public int IntValue
		{
			get
			{
				return _intValue;
			}
			set
			{
				_intValue = value;
			}
		}

		private Guid _guidValue;

		[Browsable(false)]
		public Guid GuidValue
		{
			get
			{
				return _guidValue;
			}
			set
			{
				_guidValue = value;
			}
		}

		private string _stringValue;

		[Browsable(false)]
		public string StringValue
		{
			get
			{
				return _stringValue;
			}
			set
			{
				_stringValue = value;
			}
		}

		bool _booleanValue = false;

		[Browsable(false)]
		public bool BooleanValue
		{
			get
			{
				return _booleanValue;
			}
			set
			{
				_booleanValue = value;
			}
		}

		decimal _currencyValue = 0;

		[Browsable(false)]
		public decimal CurrencyValue
		{
			get
			{
				return _currencyValue;
			}
			set
			{
				_currencyValue = value;
			}
		}

		decimal _floatValue = 0;

		[Browsable(false)]
		public decimal FloatValue
		{
			get
			{
				return _floatValue;
			}
			set
			{
				_floatValue = value;
			}
		}

		object _dateValue = null;

		[Browsable(false)]
		public object DateValue
		{
			get
			{
				return _dateValue;
			}
			set
			{
				if(value == null | value is DateTime)
				{
					_dateValue = value;
				}
				else
				{
					throw new ArgumentOutOfRangeException("DateValue", value, ResourceUtils.GetString("ErrorNotDateTime"));
				}
			}
		}

		[TypeConverter(typeof(StateMachineStateLookupReaderConverter))]
        [XmlAttribute("value")]
		public object Value
		{
			get
			{
				StateMachine sm = this.RootItem as StateMachine;
				// state machine can be null if the element was not added to the state yet
				// while designing the state machine in Architect
				if(sm != null && sm.Field != null)
				{
					StateMachineState targetState = this;
					if(this.Type == StateMachineStateType.Group)
					{
						if(this.DefaultSubstate == null)
						{
							throw new ArgumentNullException("DefaultSubstate", ResourceUtils.GetString("ErrorDefaultSubstateNotSet", sm.Name, this.Name));
						}

						targetState = this.DefaultSubstate;
					}

					return DataConstant.ConvertValue(sm.Field.DataType, targetState.StringValue, targetState.IntValue, targetState.GuidValue, targetState.CurrencyValue, targetState.FloatValue, targetState.BooleanValue, targetState.DateValue);
				}
				else
				{
					return null;
				}
			}
			set
			{
				if(this.Type == StateMachineStateType.Group)
				{
                    return;
				}

				StateMachine sm = this.RootItem as StateMachine;
				if(sm.Field != null)
				{
					switch(sm.Field.DataType)
					{
						case OrigamDataType.Xml:
						case OrigamDataType.Memo:
						case OrigamDataType.String:
							string stringValue = Convert.ToString(value);
							this.StringValue = stringValue;
							break;

						case OrigamDataType.Integer:
							int intValue = Convert.ToInt32(value);
							this.IntValue = intValue;
							break;

						case OrigamDataType.Currency:
							decimal currencyValue = Convert.ToDecimal(value);
							this.CurrencyValue = currencyValue;
							break;

						case OrigamDataType.Float:
							decimal floatValue = Convert.ToDecimal(value);
							this.FloatValue = floatValue;
							break;

						case OrigamDataType.Boolean:
							bool booleanValue = Convert.ToBoolean(value);
							this.BooleanValue = booleanValue;
							break;

						case OrigamDataType.Date:
							if(value == null)
							{
								this.DateValue = null;
							}
							else
							{
								DateTime dateValue = Convert.ToDateTime(value);
								this.DateValue = dateValue;
							}
							break;

						case OrigamDataType.UniqueIdentifier:
							Guid guidValue;

							if(value == null)
							{
								guidValue = Guid.Empty;
							}
							else
							{	
								switch(value.GetType().ToString())
								{
									case "System.String":
										guidValue = new Guid((string)value);
										break;
									case "System.Guid":
										guidValue = (Guid)value;
										break;
									default:
										throw new ArgumentOutOfRangeException("value", value, ResourceUtils.GetString("ErrorConvertToGuid"));
								}
							}

							this.GuidValue = guidValue;
							break;
					}
				}
			}
		}
		
		public Guid DefaultSubstateId;

		[TypeConverter(typeof(StateMachineSubstateConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("defaultSubstate", "DefaultSubstateId")]
		public StateMachineState DefaultSubstate
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.DefaultSubstateId;

				return (StateMachineState)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.DefaultSubstateId = Guid.Empty;
				}
				else
				{
					this.DefaultSubstateId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
									  typeof(StateMachineState),
									  typeof(StateMachineOperation)
								  };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(StateMachineState))
			{
				if(this.Type != StateMachineStateType.Group)
				{
					throw new ArgumentOutOfRangeException("Type", this.Type, ResourceUtils.GetString("ErrorAddSubstate"));
				}

				item = new StateMachineState(schemaExtensionId);
				item.Name = "NewSubState";
			}
			else if(type == typeof(StateMachineOperation))
			{
				item = new StateMachineOperation(schemaExtensionId);
				item.Name = "NewOperation";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorStateMachineStateUknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion
	}
}
