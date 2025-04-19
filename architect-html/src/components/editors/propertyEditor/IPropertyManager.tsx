import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import { IUpdatePropertiesResult } from "src/API/IArchitectApi.ts";

export interface IPropertyManager {
  onPropertyUpdated(property: EditorProperty, value: any): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult>
}