export type IScreenXml =
  | IWindow
  | IBinding
  | IGrid
  | IGridProperty
  | IDropDownProperty;

export interface IWindow {
  name: "Window";
  attributes: {
    Title: string;
    MenuId: string;
    ShowInfoPanel: string;
    AutoRefreshInterval: string;
    CacheOnClient: string;
    autoSaveListOnRecordChange: string;
    RequestSaveAfterUpdate: string;
  };
  elements: IScreenXml[];
}

export interface IBinding {
  name: "Binding";
  attributes: {
    ParentId: string;
    ParentProperty: string;
    ParentEntity: string;
    ChildId: string;
    ChildProperty: string;
    ChildEntity: string;
    ChildPropertyType: string;
  };
  elements: IScreenXml[];
}

export interface IGrid {
  name: "UIElement";
  attributes: {
    Type: "Grid";
    ConfirmSelectionChange: string;
    DataMember: string;
    DefaultPanelView: string;
    DisableActionButtons: string;
    Entity: string;
    Id: string;
    IsDraggingEnabled: string;
    IsGridHeightDynamic: string;
    IsHeadless: string;
    IsPreloaded: string;
    IsRootEntity: string;
    IsRootGrid: string;
    ModelId: string;
    ModelInstanceId: string;
    Name: string;
    OrderMember: string;
    RequestDataAfterSelectionChange: string;
    SelectionMember: string;
    ShowAddButton: string;
    ShowDeleteButton: string;
    ShowSelectionCheckboxes: string;
    Height: string | undefined;
  };
  elements: IScreenXml[];
}

export interface IGridProperty {
  name: "Property";
  attributes: {
    Column: string;
    Entity: string;
    Id: string;
    Name: string;
    AllowReturnToForm: string | undefined;
    AutoSort: string | undefined;
    Cached: string | undefined;
    CaptionLength: string | undefined;
    CaptionPosition: string | undefined;
    DropDownShowUniqueValues: string | undefined;
    DropDownType: string | undefined;
    EntityName: string | undefined;
    Height: string | undefined;
    Identifier: string | undefined;
    IdentifierIndex: string | undefined;
    IsTree: string | undefined;
    LookupId: string | undefined;
    ModelInstanceId: string | undefined;
    ReadOnly: string | undefined;
    SearchByFirstColumnOnly: string | undefined;
    Width: string | undefined;
    X: string | undefined;
    Y: string | undefined;
  };
  children: IScreenXml[];
}

export interface IDropDownProperty {
  name: "Property";
  attributes: {
    Id: string;
    Name: string;
    Entity: string;
    Column: string;
    Index: string;
  };
  elements: IScreenXml[];
}
