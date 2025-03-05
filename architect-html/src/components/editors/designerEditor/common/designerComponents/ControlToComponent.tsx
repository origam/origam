import { IApiControl } from "src/API/IArchitectApi.ts";
import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  ComponentType,
  parseComponentType
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import {
  AsCombo,
  AsDateBox,
  AsTextBox,
  Component,
  GroupBox,
  TagInput,
  TextArea
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";
import { ReactElement } from "react";
import {
  SplitPanel
} from "src/components/editors/designerEditor/common/designerComponents/SplitPanel.tsx";
import {
  AsForm
} from "src/components/editors/designerEditor/common/designerComponents/AsForm.tsx";
import {
  AsPanel
} from "src/components/editors/designerEditor/common/designerComponents/AsPanel.tsx";
import {
  FormPanel
} from "src/components/editors/designerEditor/common/designerComponents/FormPanel.tsx";
import {
  AsCheckBox
} from "src/components/editors/designerEditor/common/designerComponents/AsCheckBox.tsx";
import {
  TabControl, TabPage
} from "src/components/editors/designerEditor/common/designerComponents/TabControl.tsx";

export async function controlToComponent(
  control: IApiControl,
  parent: Component | null,
  getChildren?: (component: Component) => Component[],
  loadComponent?: (componentId: string) => Promise<ReactElement>,
): Promise<Component> {
  const properties = control.properties.map(prop => new EditorProperty(prop));
  const componentType = parseComponentType(control.type);
  switch (componentType) {
    case ComponentType.AsCombo:
      return new AsCombo({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.AsTextBox:
      return new AsTextBox({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.TagInput:
      return new TagInput({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.AsPanel:
      return new AsPanel({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.AsDateBox:
      return new AsDateBox({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.AsCheckBox:
      return new AsCheckBox({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.GroupBox:
      return new GroupBox({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.TextArea:
      return new TextArea({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.AsForm:
      return new AsForm({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.FormPanel: {
      if (!loadComponent) {
        throw new Error("loadComponent parameter is missing");
      }
      const reactElement = await loadComponent(control.id);
      return new FormPanel({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
        reactElement: reactElement
      })
    }

    case ComponentType.SplitPanel:
      if (!getChildren) {
        throw new Error("getChildren parameter is missing");
      }
      return new SplitPanel({
        id: control.id,
        parent: parent,
        getChildren: getChildren,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.TabPage:
      if (!getChildren) {
        throw new Error("getChildren parameter is missing");
      }
      return new TabPage({
        id: control.id,
        parent: parent as TabControl,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
        getChildren: getChildren,
      })

    case ComponentType.TabControl:
      return new TabControl({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    default:
      throw new Error(`Unknown component type: ${componentType}`)
  }
}

export async function toComponentRecursive(
  control: IApiControl,
  parent: Component | null,
  allComponents: Component[],
  getChildren?: (component: Component) => Component[],
  loadComponent?: (componentId: string) => Promise<ReactElement>
): Promise<Component[]> {
  const newComponent = await controlToComponent(
    control,
    parent,
    getChildren,
    loadComponent);
  for (const childControl of control.children) {
    await toComponentRecursive(
      childControl, newComponent, allComponents, getChildren, loadComponent);
  }
  allComponents.push(newComponent);
  return allComponents;
}
