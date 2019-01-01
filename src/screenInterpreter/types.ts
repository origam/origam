export interface IProperty {
  id: string;
  modelInstanceId: string;
  name: string;
  entity: string;
  column: string;
  readOnly: boolean;
  x?: number;
  y?: number;
  w?: number;
  h?: number;
  captionLength: number;
  captionPosition: string;
  lookupId?: string;
  lookupIdentifier?: string;
  tooltip?: string;
  dropDownColumns: IDropDownColumn[];
}

export interface IDropDownColumn {
  id: string;
  name: string;
  entity: string;
  column: string;
  index: number;
}

export interface IXmlNode {
  type: string;
  name: string;
  elements: IXmlNode[];
  attributes: { [key: string]: string };
  text: string;
}

// Maps [UIElement ID, Property ID] -> Property


export interface ICollectPropertiesContext {
  properties: IProperty[];
}

export interface ICollectDropDownColumnsContext {
  dropDownColumns: IDropDownColumn[];
}


