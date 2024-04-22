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
using Origam.DA.Service;
using Origam.Gui;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;
using Origam.Schema;


namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// An instance of UIElementRenderData holds necessary information for generating of AsPanel's XML.
/// </summary>
public class UIElementRenderData
{
	public string DataMember = "";

	public bool IsGridVisible = false;

	public bool HideNavigationPanel = false;

	public bool DisableActionButtons = false;

	public string PanelTitle = "";

	public string Text = "";

	public bool ShowNewButton = false;

	public bool HideCopyButton = false;

	public bool ShowDeleteButton = false;

	public string IdColumn = "";

	public string ParentIdColumn = "";

	public string NameColumn = "";

	public bool FixedSize = false;

	public int Top = 0;

	public int Left = 0;

	public int Width = 0;

	public int Height = 0;

	public int TabIndex = 0;

	public bool ButtonsOnly = false;

	public string ReportId = "";

	public string WorkflowId = "";

	public string SelectionMember = "";

	public string CalendarDateDueMember = "";

	public string CalendarDateFromMember = "";

	public string CalendarDateToMember = "";

	public string CalendarNameMember = "";

	public string CalendarDescriptionMember = "";

	public string CalendarIsFinishedMember = "";

	public string CalendarResourceIdMember = "";

	public string CalendarResourceNameLookupField = "";

	public bool IsCalendarSupported = false;

	public bool IsCalendarVisible = false;

	public int DefaultCalendarView = 0;

	public int Orientation = 0;

	public string PipelineNameMember = "";

	public string PipelineDateMember = "";

	public string PipelinePriceMember = "";

	public string PipelineStateMember = "";

	public Guid PipelineStateLoookup  = Guid.Empty;

	public bool IsPipelineSupported = false;

	public bool IsPipelineVisible = false;

	public int IndentLevel = 0;

	public bool IsHeightFixed = false;

	public bool IsOpen = false;

	public bool IsGridHeightDynamic = false;

	public int MaxDynamicGridHeight = 0;

	public Guid IndependentDataSourceId = Guid.Empty;

	public Guid IndependentDataSourceFilterId = Guid.Empty;

	public Guid IndependentDataSourceSortId = Guid.Empty;

	public Guid TreeId = Guid.Empty;

	public int TopCell = 0;

	public int LeftCell = 0;

	public int HeightCells = 0;

	public int WidthCells = 0;

	public bool IsMapSupported = false;

	public bool IsMapVisible = false;

	public string MapLocationMember = "";

	public string MapAzimuthMember = "";

	public string MapColorMember = "";

	public string MapIconMember = "";

	public string MapTextMember = "";

	public string MapTextColorMember = "";

	public string MapTextLocationMember = "";

	public string MapTextRotationMember = "";

	public string MapLayers = "";

	public string FormParameterName = "";

	public string DefaultConfiguration = "";

	public bool NewRecordInDetailView = false;

	public string ImplicitFilter = null;

	public string CalendarCustomSortMember = "";

	public Guid CalendarRowHeightConstantId = Guid.Empty;
		
	public bool IsDraggingEnabled = false;

	public string DraggingLabelMember = "";

	public bool CalendarShowAllResources = false;

	public bool IsVisualEditorSupported = false;

	public bool IsVisualEditorVisible = false;

	public WorkflowExecutionType ActionType = WorkflowExecutionType.NoFormMerge;

	public string OrderMember = "";

	public UIStyle Style = null;

	public UIStyle CalendarViewStyle = null;

	public bool AllowNavigation { get; set; }

	public Dictionary<string, string> DynamicProperties =
		new Dictionary<string, string>();
        
	public static UIElementRenderData GetRenderData(ControlSetItem control, bool forceReadOnly)
	{
			UIElementRenderData renderData = new UIElementRenderData();
			foreach(PropertyValueItem property in control.ChildItemsByType(PropertyValueItem.CategoryConst))
			{
				string stringValue = property.Value;
				if(stringValue != null && DatasetGenerator.IsCaptionExpression(stringValue))
				{
					stringValue = DatasetGenerator.EvaluateCaptionExpression(stringValue);
				}

				switch(property.ControlPropertyItem.Name)
				{
					case "GridVisible":				
						renderData.IsGridVisible = property.BoolValue;						
						break;
					case "PanelTitle":				
						renderData.PanelTitle = stringValue;								
						break;
					case "Text":					
						renderData.Text = stringValue;										
						break;
					case "HideNavigationPanel":		
						renderData.HideNavigationPanel = property.BoolValue;				
						break;
					case "DisableActionButtons":	
						renderData.DisableActionButtons = property.BoolValue;				
						break;
					case "DataMember":				
						renderData.DataMember = property.Value;
						break;
					case "ShowNewButton":			
						renderData.ShowNewButton = (forceReadOnly ? false : property.BoolValue);	
						break;
					case "HideCopyButton":			
						renderData.HideCopyButton = property.BoolValue;	
						break;
					case "ShowDeleteButton":		
						renderData.ShowDeleteButton = (forceReadOnly ? false : property.BoolValue); 
						break;
					case "IDColumn":				
						renderData.IdColumn = property.Value;
						break;
					case "ParentIDColumn":			
						renderData.ParentIdColumn = property.Value;
						break;
					case "NameColumn":				
						renderData.NameColumn = property.Value;							
						break;
					case "FixedSize":				
						renderData.FixedSize = property.BoolValue;							
						break;
					case "Top":						
						renderData.Top = property.IntValue;								
						break;
					case "Left":					
						renderData.Left = property.IntValue;								
						break;
					case "Width":					
						renderData.Width = property.IntValue;								
						break;
					case "Height":					
						renderData.Height = property.IntValue;
						break;
					case "TabIndex":				
						renderData.TabIndex = property.IntValue;							
						break;
					case "ButtonsOnly":				
						renderData.ButtonsOnly = property.BoolValue;
						break;
					case "ReportId":				
						renderData.ReportId = property.GuidValue.ToString();				
						break;
					case "WorkflowId":				
						renderData.WorkflowId = property.GuidValue.ToString();				
						break;
					case "ActionType":				
						renderData.ActionType = (WorkflowExecutionType)property.IntValue;				
						break;
					case "SelectionMember":			
						renderData.SelectionMember = property.Value;
						break;
					case "CalendarDateDueMember":	
						renderData.CalendarDateDueMember = property.Value;					
						break;
					case "CalendarDateFromMember":	
						renderData.CalendarDateFromMember = property.Value;				
						break;
					case "CalendarDateToMember":	
						renderData.CalendarDateToMember = property.Value;					
						break;
					case "CalendarNameMember":		
						renderData.CalendarNameMember = property.Value;
						break;
					case "CalendarDescriptionMember": 
						renderData.CalendarDescriptionMember = property.Value;			
						break;
					case "CalendarIsFinishedMember":  
						renderData.CalendarIsFinishedMember = property.Value;			
						break;
					case "CalendarResourceIdMember":  
						renderData.CalendarResourceIdMember = property.Value;			
						break;
					case "CalendarResourceNameLookupField":  
						renderData.CalendarResourceNameLookupField = property.Value;			
						break;
					case "IsCalendarSupported":		
						renderData.IsCalendarSupported = property.BoolValue;				
						break;
					case "IsCalendarVisible":		
						renderData.IsCalendarVisible = property.BoolValue;					
						break;
					case "DefaultCalendarView":		
						renderData.DefaultCalendarView = property.IntValue;				
						break;
					case "Orientation":				
						renderData.Orientation = property.IntValue;						
						break;
					case "PipelineNameMember":		
						renderData.PipelineNameMember = property.Value;					
						break;
					case "PipelineDateMember":		
						renderData.PipelineDateMember = property.Value;					
						break;
					case "PipelinePriceMember":		
						renderData.PipelinePriceMember = property.Value;					
						break;
					case "PipelineStateMember":		
						renderData.PipelineStateMember = property.Value;					
						break;
					case "PipelineStateLookupId":	
						renderData.PipelineStateLoookup = property.GuidValue;				
						break;
					case "IsPipelineSupported":		
						renderData.IsPipelineSupported = property.BoolValue;				
						break;
					case "IsPipelineVisible":		
						renderData.IsPipelineVisible = property.BoolValue;					
						break;
					case "IndentLevel":				
						renderData.IndentLevel = property.IntValue;						
						break;
					case "IsHeightFixed":			
						renderData.IsHeightFixed = property.BoolValue;						
						break;
					case "IsOpen":					
						renderData.IsOpen = property.BoolValue;							
						break;
					case "IsGridHeightDynamic":		
						renderData.IsGridHeightDynamic = property.BoolValue;				
						break;
					case "MaxDynamicGridHeight":	
						renderData.MaxDynamicGridHeight = property.IntValue;				
						break;
					case "IndependentDataSourceId":	
						renderData.IndependentDataSourceId = property.GuidValue;			
						break;
					case "IndependentDataSourceFilterId":	
						renderData.IndependentDataSourceFilterId = property.GuidValue;	
						break;
					case "IndependentDataSourceSortId":	
						renderData.IndependentDataSourceSortId = property.GuidValue;	
						break;
					case "TopCell":					
						renderData.TopCell = property.IntValue;
						break;
					case "LeftCell":				
						renderData.LeftCell = property.IntValue;							
						break;
					case "WidthCells":				
						renderData.WidthCells = property.IntValue;							
						break;
					case "HeightCells":				
						renderData.HeightCells = property.IntValue;						
						break;
					case "IsMapSupported":			
						renderData.IsMapSupported = property.BoolValue;					
						break;
					case "IsMapVisible":			
						renderData.IsMapVisible = property.BoolValue;						
						break;
					case "MapLocationMember":		
						renderData.MapLocationMember = property.Value;						
						break;
					case "MapAzimuthMember":		
						renderData.MapAzimuthMember = property.Value;						
						break;
					case "MapColorMember":			
						renderData.MapColorMember = property.Value;						
						break;
					case "MapIconMember":			
						renderData.MapIconMember = property.Value;							
						break;
					case "MapTextMember":			
						renderData.MapTextMember = property.Value;							
						break;
					case "MapTextColorMember":		
						renderData.MapTextColorMember = property.Value;					
						break;
					case "MapTextLocationMember":	
						renderData.MapTextLocationMember = property.Value;					
						break;
					case "MapTextRotationMember":	
						renderData.MapTextRotationMember = property.Value;					
						break;
					case "MapLayers":				
						renderData.MapLayers = property.Value;								
						break;
					case "FormParameterName":		
						renderData.FormParameterName = property.Value;						
						break;
					case "TreeId":					
						renderData.TreeId = property.GuidValue;							
						break;
					case "DefaultConfiguration":	
						renderData.DefaultConfiguration = property.Value;					
						break;
					case "NewRecordInDetailView":	
						renderData.NewRecordInDetailView = property.BoolValue;				
						break;
					case "ImplicitFilter":			
						renderData.ImplicitFilter = property.Value;						
						break;
					case "CalendarCustomSortMember": 
						renderData.CalendarCustomSortMember = property.Value;				
						break;
					case "CalendarRowHeightConstantId": 
						renderData.CalendarRowHeightConstantId = property.GuidValue;	
						break;
					case "IsDraggingEnabled":		
						renderData.IsDraggingEnabled = property.BoolValue;					
						break;
					case "DraggingLabelMember":		
						renderData.DraggingLabelMember = property.Value;					
						break;
					case "CalendarShowAllResources": 
						renderData.CalendarShowAllResources = property.BoolValue;			
						break;
					case "IsVisualEditorSupported":	
						renderData.IsVisualEditorSupported = property.BoolValue;			
						break;
					case "IsVisualEditorVisible":	
						renderData.IsVisualEditorVisible = property.BoolValue;				
						break;
					case "OrderMember":
						renderData.OrderMember = property.Value;
						break;					
					case "AllowNavigation":
						renderData.AllowNavigation = property.BoolValue;
						break;
                    case "StyleId":
                        if (!property.GuidValue.Equals(Guid.Empty))
                        {
                            renderData.Style = GetStyle(property);
                        }
                        break;
                    case "CalendarViewStyleId":
                        if (!property.GuidValue.Equals(Guid.Empty))
                        {
                            renderData.CalendarViewStyle = GetStyle(property);
                        }
                        break;
                    default:
	                    renderData.DynamicProperties[property.Name] = property.Value;
	                    break;
                }
			}
			return renderData;
		}

	private static UIStyle GetStyle(PropertyValueItem property)
	{
            IPersistenceService persistence = ServiceManager.Services.GetService(
                typeof(IPersistenceService)) as IPersistenceService;
            return persistence.SchemaProvider.RetrieveInstance(
                typeof(UIStyle), new ModelElementKey(property.GuidValue)) as UIStyle;
        }
}