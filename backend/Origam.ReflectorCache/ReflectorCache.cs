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

namespace Origam.ReflectorCache;
/// <summary>
/// Summary description for Class1.
/// </summary>
public class ReflectorCache : IReflectorCache
{
	public ReflectorCache()
	{
	}
	public object InvokeObject(string classname, string assembly)
	{
#if !ORIGAM_SERVER && !NETSTANDARD
		switch(assembly)
		{
			case "Origam.Gui.Win":
				switch(classname)
				{
					case "Origam.Gui.Win.AsPanel":
						return new Origam.Gui.Win.AsPanel();
					case "Origam.Gui.Win.AsForm":
						return new Origam.Gui.Win.AsForm();
					case "Origam.Gui.Win.GroupBoxWithChamfer":
						return new Origam.Gui.Win.GroupBoxWithChamfer();
					
					case "Origam.Gui.Win.AsDropDown":
						return new Origam.Gui.Win.AsDropDown();
					
					case "Origam.Gui.Win.AsDateBox":
						return new Origam.Gui.Win.AsDateBox();
					
					case "Origam.Gui.Win.AsTextBox":
						return new Origam.Gui.Win.AsTextBox();
				}
				break;
//				case "System.Windows.Forms":
//					switch(classname)
//					{
//						case "System.Windows.Forms.Label":
//							return;
//						case "System.Windows.Forms.Panel":
//							return;
//					}
//					break;
		}
#endif
        return null;
	}
	public object InvokeObject(string typeName, object[] args)
	{
		var key = args[0] as Key;
		switch(typeName)
		{
			case "Origam.Schema.EntityModel.DataConstantReference":
				return new Origam.Schema.EntityModel.DataConstantReference(key);
			case "Origam.Schema.GuiModel.ControlItem":
				return new Origam.Schema.GuiModel.ControlItem(key);
			case "Origam.Schema.RuleModel.ComplexDataRule":
				return new Origam.Schema.RuleModel.ComplexDataRule(key);
            case "Origam.Schema.RuleModel.EndRule":
                return new Origam.Schema.RuleModel.EndRule(key);
            case "Origam.Schema.RuleModel.EntityRule":
                return new Origam.Schema.RuleModel.EntityRule(key);
            case "Origam.Schema.RuleModel.SimpleDataRule":
                return new Origam.Schema.RuleModel.SimpleDataRule(key);
            case "Origam.Schema.RuleModel.StartRule":
                return new Origam.Schema.RuleModel.StartRule(key);
            case "Origam.Schema.EntityModel.DataStructureFilterSetFilter":
				return new Origam.Schema.EntityModel.DataStructureFilterSetFilter(key);
			case "Origam.Schema.TestModel.TestCaseStep":
				return new Origam.Schema.TestModel.TestCaseStep(key);
			case "Origam.Schema.WorkflowModel.SetWorkflowPropertyTask":
				return new Origam.Schema.WorkflowModel.SetWorkflowPropertyTask(key);
			case "Origam.Schema.WorkflowModel.UpdateContextTask":
				return new Origam.Schema.WorkflowModel.UpdateContextTask(key);
			case "Origam.Schema.WorkflowModel.AcceptContextStoreChanges":
				return new Origam.Schema.WorkflowModel.AcceptContextStoreChangesTask(key);
			case "Origam.Schema.EntityModel.DataStructureReference":
				return new Origam.Schema.EntityModel.DataStructureReference(key);
			case "Origam.Schema.GuiModel.PropertyValueItem":
				return new Origam.Schema.GuiModel.PropertyValueItem(key);
			case "Origam.Schema.EntityModel.TransformationReference":
				return new Origam.Schema.EntityModel.TransformationReference(key);
			case "Origam.Schema.EntityModel.AggregatedColumn":
				return new Origam.Schema.EntityModel.AggregatedColumn(key);
			case "Origam.Schema.WorkflowModel.ContextStoreLink":
				return new Origam.Schema.WorkflowModel.ContextStoreLink(key);
			case "Origam.Schema.GuiModel.CrystalReport":
				return new Origam.Schema.GuiModel.CrystalReport(key);
			case "Origam.Schema.WorkflowModel.WorkflowTaskDependency":
				return new Origam.Schema.WorkflowModel.WorkflowTaskDependency(key);
			case "Origam.Schema.WorkflowModel.UIFormTask":
				return new Origam.Schema.WorkflowModel.UIFormTask(key);
			case "Origam.Schema.EntityModel.FunctionCall":
				return new Origam.Schema.EntityModel.FunctionCall(key);
			case "Origam.Schema.GuiModel.ReportReference":
				return new Origam.Schema.GuiModel.ReportReference(key);
			case "Origam.Schema.EntityModel.EntityRelationFilter":
				return new Origam.Schema.EntityModel.EntityRelationFilter(key);
			case "Origam.Schema.WorkflowModel.TransactionWorkflowBlock":
				return new Origam.Schema.WorkflowModel.TransactionWorkflowBlock(key);
			case "Origam.Schema.GuiModel.FormControlSet":
				return new Origam.Schema.GuiModel.FormControlSet(key);
			case "Origam.Schema.WorkflowModel.ContextReference":
				return new Origam.Schema.WorkflowModel.ContextReference(key);
			case "Origam.Schema.SchemaItemAncestor":
				return new Origam.Schema.SchemaItemAncestor(key);
			case "Origam.Schema.WorkflowModel.SystemFunctionCall":
				return new Origam.Schema.WorkflowModel.SystemFunctionCall(key);
			case "Origam.Schema.ParameterReference":
				return new Origam.Schema.ParameterReference(key);
			case "Origam.Schema.EntityModel.EntityColumnReference":
				return new Origam.Schema.EntityModel.EntityColumnReference(key);
			case "Origam.Schema.WorkflowModel.WorkflowCallTask":
				return new Origam.Schema.WorkflowModel.WorkflowCallTask(key);
			case "Origam.Schema.DeploymentModel.DeploymentVersion":
				return new Origam.Schema.DeploymentModel.DeploymentVersion(key);
			case "Origam.Schema.EntityModel.TableMappingItem":
				return new Origam.Schema.EntityModel.TableMappingItem(key);
			case "Origam.Schema.MenuModel.FormReferenceMenuItem":
				return new Origam.Schema.MenuModel.FormReferenceMenuItem(key);
			case "Origam.Schema.EntityModel.DataStructureEntity":
				return new Origam.Schema.EntityModel.DataStructureEntity(key);
			case "Origam.Schema.SchemaItemParameter":
				return new Origam.Schema.SchemaItemParameter(key);
			case "Origam.Schema.EntityModel.DataConstant":
				return new Origam.Schema.EntityModel.DataConstant(key);
			case "Origam.Schema.WorkflowModel.Workflow":
				return new Origam.Schema.WorkflowModel.Workflow(key);
			case "Origam.Schema.TestModel.TestCase":
				return new Origam.Schema.TestModel.TestCase(key);
			case "Origam.Schema.WorkflowModel.ServiceMethodCallTask":
				return new Origam.Schema.WorkflowModel.ServiceMethodCallTask(key);
			case "Origam.Schema.EntityModel.DataEntityIndexField":
				return new Origam.Schema.EntityModel.DataEntityIndexField(key);
			case "Origam.Schema.GuiModel.PropertyBindingInfo":
				return new Origam.Schema.GuiModel.PropertyBindingInfo(key);
			case "Origam.Schema.GuiModel.ControlPropertyItem":
				return new Origam.Schema.GuiModel.ControlPropertyItem(key);
			case "Origam.Schema.LookupModel.DataServiceDataLookup":
				return new Origam.Schema.LookupModel.DataServiceDataLookup(key);
			case "Origam.Schema.WorkflowModel.ServiceMethodCallParameter":
				return new Origam.Schema.WorkflowModel.ServiceMethodCallParameter(key);
			case "Origam.Schema.EntityModel.EntityFilterReference":
				return new Origam.Schema.EntityModel.EntityFilterReference(key);
			case "Origam.Schema.Package":
				return new Origam.Schema.Package(key);
			case "Origam.Schema.WorkflowModel.ForeachWorkflowBlock":
				return new Origam.Schema.WorkflowModel.ForeachWorkflowBlock(key);
			case "Origam.Schema.WorkflowModel.ContextStore":
				return new Origam.Schema.WorkflowModel.ContextStore(key);
			case "Origam.Schema.EntityModel.FieldMappingItem":
				return new Origam.Schema.EntityModel.FieldMappingItem(key);
			case "Origam.Schema.EntityModel.FunctionParameter":
				return new Origam.Schema.EntityModel.FunctionParameter(key);
			case "Origam.Schema.EntityModel.Function":
				return new Origam.Schema.EntityModel.Function(key);
			case "Origam.Schema.SchemaItemGroup":
				return new Origam.Schema.SchemaItemGroup(key);
			case "Origam.Schema.GuiModel.PanelControlSet":
				return new Origam.Schema.GuiModel.PanelControlSet(key);
			case "Origam.Schema.EntityModel.XsdDataStructure":
				return new Origam.Schema.EntityModel.XsdDataStructure(key);
			case "Origam.Schema.EntityModel.FunctionCallParameter":
				return new Origam.Schema.EntityModel.FunctionCallParameter(key);
			case "Origam.Schema.EntityModel.XslTransformation":
				return new Origam.Schema.EntityModel.XslTransformation(key);
			case "Origam.Schema.MenuModel.WorkflowReferenceMenuItem":
				return new Origam.Schema.MenuModel.WorkflowReferenceMenuItem(key);
			case "Origam.Schema.EntityModel.DataStructureColumn":
				return new Origam.Schema.EntityModel.DataStructureColumn(key);
			case "Origam.Schema.EntityModel.EntityRelationItem":
				return new Origam.Schema.EntityModel.EntityRelationItem(key);
			case "Origam.Schema.GuiModel.ControlSetItem":
				return new Origam.Schema.GuiModel.ControlSetItem(key);
			case "Origam.Schema.GuiModel.ColumnParameterMapping":
				return new Origam.Schema.GuiModel.ColumnParameterMapping(key);
			case "Origam.Schema.WorkflowModel.ServiceMethod":
				return new Origam.Schema.WorkflowModel.ServiceMethod(key);
			case "Origam.Schema.EntityModel.EntityRelationColumnPairItem":
				return new Origam.Schema.EntityModel.EntityRelationColumnPairItem(key);
			case "Origam.Schema.MenuModel.Menu":
				return new Origam.Schema.MenuModel.Menu(key);
		}
		return null;
	}
	public bool SetValue(object instance, string property, object value)
	{
		if(instance is Origam.Schema.ISchemaItem)
		{
			switch(property)
			{
				case "IsAbstract":
					(instance as Origam.Schema.ISchemaItem).IsAbstract = (System.Boolean)value;
					return true;
				case "Name":
					(instance as Origam.Schema.ISchemaItem).Name = value == null ? null : (string)value;
					return true;
				case "ParentItemId":
					(instance as Origam.Schema.ISchemaItem).ParentItemId = value == null ? Guid.Empty : (Guid)value;
					return true;
				case "SchemaExtensionId":
					(instance as Origam.Schema.ISchemaItem).SchemaExtensionId = value == null ? Guid.Empty : (Guid)value;
					return true;
				case "GroupId":
					(instance as Origam.Schema.ISchemaItem).GroupId = value == null ? Guid.Empty : (Guid)value;
					return true;
			}
		}
		if(instance is Origam.Schema.SchemaItemAncestor)
		{
			switch(property)
			{
				case "AncestorId":
					(instance as Origam.Schema.SchemaItemAncestor).AncestorId = value == null ? Guid.Empty : (Guid)value;
					return true;
			}
		}
		if(instance is Origam.Schema.GuiModel.ColumnParameterMapping)
		{
			switch(property)
			{
				case "ColumnName":
					(instance as Origam.Schema.GuiModel.ColumnParameterMapping).ColumnName = value == null ? null : (string)value;
					return true;
			}
		}
		if(instance is Origam.Schema.GuiModel.AbstractPropertyValueItem)
		{
			switch(property)
			{
				case "ControlPropertyId":
                    if (value is string)
                    {
                        value = new Guid((string)value);
                    }
                    (instance as Origam.Schema.GuiModel.AbstractPropertyValueItem).ControlPropertyId = value == null ? Guid.Empty : (Guid)value;
					return true;
			}
		}
		if(instance is Origam.Schema.GuiModel.PropertyBindingInfo)
		{
			switch(property)
			{
				case "DesignDataSetPath":
					(instance as Origam.Schema.GuiModel.PropertyBindingInfo).DesignDataSetPath = value == null ? null : (string)value;
					return true;
                case "Value":
                    (instance as Origam.Schema.GuiModel.PropertyBindingInfo).Value = value == null ? null : (string)value;
                    return true;
            }
        }
		if(instance is Origam.Schema.GuiModel.ControlPropertyItem)
		{
			switch(property)
			{
				case "IsBindOnly":
					(instance as Origam.Schema.GuiModel.ControlPropertyItem).IsBindOnly = (System.Boolean)value;
					return true;
				case "PropertyType":
					(instance as Origam.Schema.GuiModel.ControlPropertyItem).PropertyType = (Origam.Schema.GuiModel.ControlPropertyValueType)value;
					return true;
			}
		}
		if(instance is Origam.Schema.GuiModel.PropertyValueItem)
		{
			switch(property)
			{
				case "BoolValue":
					(instance as Origam.Schema.GuiModel.PropertyValueItem).BoolValue = value == null ? false : (bool)value;
					return true;
				case "IntValue":
					(instance as Origam.Schema.GuiModel.PropertyValueItem).IntValue = value == null ? 0 : (int)value;
					return true;
				case "GuidValue":
                    if (value is string)
                    {
                        value = new Guid((string)value);
                    }
                    (instance as Origam.Schema.GuiModel.PropertyValueItem).GuidValue = value == null ? Guid.Empty : (Guid)value;
					return true;
                case "Value":
                    (instance as Origam.Schema.GuiModel.PropertyValueItem).Value = value == null ? null : (string)value;
                    return true;
            }
        }
		return false;
	}
}
