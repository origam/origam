import { IApiControl } from "src/API/IArchitectApi.ts";
import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  ComponentType,
  parseComponentType
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import {
  AsCheckBox,
  AsCombo,
  AsDateBox,
  AsForm,
  AsPanel,
  AsTextBox,
  Component,
  FormPanel,
  GroupBox,
  SplitPanel, TabControl, TabPage,
  TagInput,
  TextArea
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";
import { ReactElement } from "react";
import {
  SectionItem
} from "src/components/editors/designerEditor/common/SectionItem.tsx";

export function controlToComponent(
  control: IApiControl,
  parent: Component | null,
): Component {
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

    case ComponentType.FormPanel:
      return new FormPanel({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.SplitPanel:
      return new SplitPanel({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
      })

    case ComponentType.TabPage:
      return new TabPage({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties
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

export function sectionToComponent(
  sectionRootControl: IApiControl
): ReactElement {
  const sectionComponents = toComponentRecursive(sectionRootControl, null, [])
  return <SectionItem components={sectionComponents}/>
}

export function toComponentRecursive(
  control: IApiControl,
  parent: Component | null,
  allComponents: Component[]
): Component[] {
  const newComponent = controlToComponent(control, parent);
  for (const childControl of control.children) {
    toComponentRecursive(childControl, newComponent, allComponents);
  }
  allComponents.push(newComponent);
  return allComponents;
}
