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
using System.Linq;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel;

public enum StateMachineStateType
{
    Initial = 0,
    Running = 1,
    Final = 2,
    Group = 4,
}

[SchemaItemDescription("State", "States", "state.png")]
[HelpTopic("State")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class StateMachineState : AbstractSchemaItem
{
    public const string CategoryConst = "StateMachineState";

    public StateMachineState() { }

    public StateMachineState(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public StateMachineState(Key primaryKey)
        : base(primaryKey) { }

    #region Overriden ISchemaItem Members

    public override string ItemType => CategoryConst;
    public override string Icon
    {
        get
        {
            switch (Type)
            {
                case StateMachineStateType.Initial:
                {
                    return "state-initial.png";
                }
                case StateMachineStateType.Running:
                {
                    return "state-running.png";
                }
                case StateMachineStateType.Final:
                {
                    return "state-final.png";
                }
                case StateMachineStateType.Group:
                {
                    return "state-group.png";
                }
                default:
                {
                    return "state.png";
                }
            }
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (DefaultSubstate != null)
        {
            dependencies.Add(DefaultSubstate);
        }
        base.GetExtraDependencies(dependencies);
    }

    public override bool CanMove(UI.IBrowserNode2 newNode)
    {
        // move between same parent type withing the same state machine
        return ((ISchemaItem)newNode).RootItem.PrimaryKey.Equals(RootItem.PrimaryKey)
            && (
                ((ISchemaItem)newNode).ItemType == "WorkflowStateMachine"
                || (((ISchemaItem)newNode).ItemType == "StateMachineState")
            );
    }
    #endregion
    #region Properties
    [Browsable(false)]
    public List<StateMachineOperation> Operations =>
        ChildItemsByType<StateMachineOperation>(StateMachineOperation.CategoryConst);

    [Browsable(false)]
    public List<StateMachineState> SubStates
    {
        get
        {
            var result = new List<StateMachineState>();
            foreach (ISchemaItem item in ChildItemsRecursive)
            {
                if (item is StateMachineState state)
                {
                    result.Add(state);
                }
            }
            return result;
        }
    }

    public bool IsState(object value)
    {
        return Value.Equals(value) || SubStates.Any(state => state.Value.Equals(value));
    }

    private StateMachineStateType _type;

    [XmlAttribute("type")]
    public StateMachineStateType Type
    {
        get => _type;
        set => _type = value;
    }
    private int _intValue;

    [Browsable(false)]
    public int IntValue
    {
        get => _intValue;
        set => _intValue = value;
    }
    private Guid _guidValue;

    [Browsable(false)]
    public Guid GuidValue
    {
        get => _guidValue;
        set => _guidValue = value;
    }
    private string _stringValue;

    [Browsable(false)]
    public string StringValue
    {
        get => _stringValue;
        set => _stringValue = value;
    }
    bool _booleanValue = false;

    [Browsable(false)]
    public bool BooleanValue
    {
        get => _booleanValue;
        set => _booleanValue = value;
    }
    decimal _currencyValue = 0;

    [Browsable(false)]
    public decimal CurrencyValue
    {
        get => _currencyValue;
        set => _currencyValue = value;
    }
    decimal _floatValue = 0;

    [Browsable(false)]
    public decimal FloatValue
    {
        get => _floatValue;
        set => _floatValue = value;
    }
    object _dateValue = null;

    [Browsable(false)]
    public object DateValue
    {
        get => _dateValue;
        set
        {
            if ((value == null) || (value is DateTime))
            {
                _dateValue = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    "DateValue",
                    value,
                    ResourceUtils.GetString("ErrorNotDateTime")
                );
            }
        }
    }

    [TypeConverter(typeof(StateMachineStateLookupReaderConverter))]
    [XmlAttribute("value")]
    public object Value
    {
        get
        {
            // state machine can be null if the element was not added to the state yet
            // while designing the state machine in Architect
            if (RootItem is StateMachine stateMachine && stateMachine.Field != null)
            {
                var targetState = this;
                if (Type == StateMachineStateType.Group)
                {
                    if (DefaultSubstate == null)
                    {
                        throw new ArgumentNullException(
                            "DefaultSubstate",
                            ResourceUtils.GetString(
                                "ErrorDefaultSubstateNotSet",
                                stateMachine.Name,
                                Name
                            )
                        );
                    }
                    targetState = DefaultSubstate;
                }
                return DataConstant.ConvertValue(
                    stateMachine.Field.DataType,
                    targetState.StringValue,
                    targetState.IntValue,
                    targetState.GuidValue,
                    targetState.CurrencyValue,
                    targetState.FloatValue,
                    targetState.BooleanValue,
                    targetState.DateValue
                );
            }
            return null;
        }
        set
        {
            if (Type == StateMachineStateType.Group)
            {
                return;
            }
            var stateMachine = RootItem as StateMachine;
            if (stateMachine.Field != null)
            {
                switch (stateMachine.Field.DataType)
                {
                    case OrigamDataType.Xml:
                    case OrigamDataType.Memo:
                    case OrigamDataType.String:
                    {
                        var stringValue = Convert.ToString(value);
                        StringValue = stringValue;
                        break;
                    }
                    case OrigamDataType.Integer:
                    {
                        var intValue = Convert.ToInt32(value);
                        IntValue = intValue;
                        break;
                    }
                    case OrigamDataType.Currency:
                    {
                        var currencyValue = Convert.ToDecimal(value);
                        CurrencyValue = currencyValue;
                        break;
                    }
                    case OrigamDataType.Float:
                    {
                        var floatValue = Convert.ToDecimal(value);
                        FloatValue = floatValue;
                        break;
                    }
                    case OrigamDataType.Boolean:
                    {
                        var booleanValue = Convert.ToBoolean(value);
                        BooleanValue = booleanValue;
                        break;
                    }
                    case OrigamDataType.Date:
                    {
                        if (value == null)
                        {
                            DateValue = null;
                        }
                        else
                        {
                            var dateValue = Convert.ToDateTime(value);
                            DateValue = dateValue;
                        }
                        break;
                    }
                    case OrigamDataType.UniqueIdentifier:
                    {
                        Guid guidValue;
                        if (value == null)
                        {
                            guidValue = Guid.Empty;
                        }
                        else
                        {
                            switch (value.GetType().ToString())
                            {
                                case "System.String":
                                {
                                    guidValue = new Guid((string)value);
                                    break;
                                }

                                case "System.Guid":
                                {
                                    guidValue = (Guid)value;
                                    break;
                                }

                                default:
                                {
                                    throw new ArgumentOutOfRangeException(
                                        "value",
                                        value,
                                        ResourceUtils.GetString("ErrorConvertToGuid")
                                    );
                                }
                            }
                        }
                        GuidValue = guidValue;
                        break;
                    }
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
            var key = new ModelElementKey { Id = DefaultSubstateId };
            return (StateMachineState)
                PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key);
        }
        set
        {
            if (value == null)
            {
                DefaultSubstateId = Guid.Empty;
            }
            else
            {
                DefaultSubstateId = (Guid)value.PrimaryKey["Id"];
            }
        }
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes =>
        new[] { typeof(StateMachineState), typeof(StateMachineOperation) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        if (typeof(T) == typeof(StateMachineState))
        {
            if (Type != StateMachineStateType.Group)
            {
                throw new ArgumentOutOfRangeException(
                    "Type",
                    Type,
                    ResourceUtils.GetString("ErrorAddSubstate")
                );
            }
        }
        string itemName = null;
        if (typeof(T) == typeof(StateMachineState))
        {
            itemName = "NewSubState";
        }
        else if (typeof(T) == typeof(StateMachineOperation))
        {
            itemName = "NewOperation";
        }
        return base.NewItem<T>(schemaExtensionId, group, itemName);
    }
    #endregion
}
