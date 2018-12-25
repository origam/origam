import { IProperty } from "src/screenInterpreter/types";

export interface IDataViewProps {
  
  hasGridView?: boolean;
  hasFormView?: boolean;
  hasMapView?: boolean;
  properties: IProperty[];
}