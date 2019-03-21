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

public enum ModelElementPolicyCommand
{
	Show,
	Create,
	MoveToPackage,
	CustomAction
}

namespace Origam
{
	/// <summary>
	/// Summary description for LicensePolicy.
	/// </summary>
	public class LicensePolicy
	{
		public static bool ModelElementPolicy (string elementType, ModelElementPolicyCommand command)
		{
			return ModelElementPolicy(elementType, command, null);
		}
		
		public static bool ModelElementPolicy (string elementType, ModelElementPolicyCommand command, string action)
		{
#if _ARCHITECT_EXPRESS
			switch(command)
			{
				case ModelElementPolicyCommand.CustomAction:
				{
					switch(action)
					{
						case "OrigamArchitect.Commands.MakeVersionCurrent":
						case "OrigamArchitect.Commands.SaveDataFromDataStructure":
						case "OrigamArchitect.Commands.GenerateDataStructureEntityColumns":
						case "Origam.Schema.EntityModel.Wizards.CreateDataStructureFromEntityCommand":
						case "Origam.Schema.EntityModel.Wizards.CreateFilterByFieldCommand":
						case "Origam.Schema.EntityModel.Wizards.CreateFilterWithParameterByFieldCommand":
						case "Origam.Schema.LookupModel.Wizards.CreateLookupFromEntityCommand":
							return true;
						default:
							return false;
					}
				}
				case ModelElementPolicyCommand.MoveToPackage:
					return false;
				case ModelElementPolicyCommand.Show:
				switch(elementType)
				{
					case "DetachedEntity":
					case "DetachedField":
					case "EntityRelationItem":
					case "EntityRelationColumnPairItem":
					case "EntityRelationFilter":
					case "FieldMappingItem":
					case "Function":
					case "FunctionCallParameter":
					case "TableMappingItem":
					case "Menu":
					case "DataConstantReferenceMenuItem":
					case "FormReferenceMenuItem":
					case "WorkflowReferenceMenuItem":
					case "DataStructure":
					case "DataStructureEntity":
					case "DataStructureFilterSet":
					case "DataStructureFilterSetFilter":
					case "DataStructureColumn":
					case "DataStructureSortSet":
					case "DataStructureSortSetItem":
					case "DataServiceDataLookup":
					case "DataLookupMenuBinding":
					case "DataConstant":
					case "EntityFilter":
					case "EntityFilterReference":
					case "EntityColumnReference":
					case "DataConstantReference":
					case "FunctionCall":
					case "Graphics":
					case "ReportReferenceMenuItem":
					case "SelectionDialogParameterMapping":
					case "Submenu":
					case "CrystalReport":
					case "DeploymentVersion":
					case "SchemaItemParameter":
					case "ParameterReference":
						return true;
					default:
						return false;
				}
				case ModelElementPolicyCommand.Create:
				{
					switch(elementType)
					{
						case "DataStructure":
						case "DataStructureEntity":
						case "DataStructureFilterSet":
						case "DataStructureFilterSetFilter":
						case "DataStructureColumn":
						case "DataStructureSortSet":
						case "DataStructureSortSetItem":
						case "DataLookupMenuBinding":
						case "DataConstant":
						case "EntityFilter":
						case "EntityFilterReference":
						case "EntityColumnReference":
						case "DataConstantReference":
						case "FunctionCall":
						case "Graphics":
						case "ReportReferenceMenuItem":
						case "SelectionDialogParameterMapping":
						case "Submenu":
						case "CrystalReport":
						case "DeploymentVersion":
						case "SchemaItemParameter":
						case "ParameterReference":
							return true;
						default:
							return false;
					}
				}
			}

			return false;
#else
			return true;
#endif
		}

		public static bool IsModelProviderVisible (string elementType)
		{
#if _ARCHITECT_EXPRESS
			switch(elementType)
			{
				case "GraphicsSchemaItemProvider":
				case "DataConstantSchemaItemProvider":
				case "DataStructureSchemaItemProvider":
				case "EntityModelSchemaItemProvider":
				case "FunctionSchemaItemProvider":
				case "DataLookupSchemaItemProvider":
				case "MenuSchemaItemProvider":
				case "ReportSchemaItemProvider":
				case "DeploymentSchemaItemProvider":
				case "SchemaItemGroup":
					return true;
				default:
					return false;
			}
#else
			return true;
#endif
		}

		public static long ModelElementLimit(string elementType)
		{
//#if ARCHITECT_EXPRESS
//			switch(elementType)
//			{
//				case "DataEntityColumn":
//					return 5;
//				case "DataEntity":
//					return 15;
//				case "DataStructure":
//					return 30;
//				case "Workflow":
//					return 2;
//				case "Transformation":
//					return 5;
//				case "FormControlSet":
//					return 10;
//				case "PanelControlSet":
//					return 20;
//				case "Report":
//					return 1;
//				case "DataLookup":
//					return 10;
//				case "DataConstant":
//					return 5;
//				case "Rule":
//					return 20;
//				case "Menu":
//					return 1;
//				default:
//					return -1;
//			}
//#else
			if(elementType == "Menu")
			{
				return 1;
			}
			else
			{
				return -1;
			}
//#endif
		}
	}
}
