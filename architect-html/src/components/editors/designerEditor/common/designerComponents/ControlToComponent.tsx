/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

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

import { IApiControl } from '@api/IArchitectApi';
import { ComponentType, parseComponentType } from '@editors/designerEditor/common/ComponentType';
import { AsCheckBox } from '@editors/designerEditor/common/designerComponents/AsCheckBox';
import { AsForm } from '@editors/designerEditor/common/designerComponents/AsForm';
import { AsPanel } from '@editors/designerEditor/common/designerComponents/AsPanel';
import {
  AsCombo,
  AsDateBox,
  AsTextBox,
  AsTree,
  Component,
  GroupBox,
  TagInput,
  TextArea,
} from '@editors/designerEditor/common/designerComponents/Component';
import { FormPanel } from '@editors/designerEditor/common/designerComponents/FormPanel';
import { SplitPanel } from '@editors/designerEditor/common/designerComponents/SplitPanel';
import { TabControl, TabPage } from '@editors/designerEditor/common/designerComponents/TabControl';
import { EditorProperty } from '@editors/gridEditor/EditorProperty';
import { ReactElement } from 'react';
import { Label } from '@editors/designerEditor/common/designerComponents/Label.tsx';

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
        properties: properties,
      });

    case ComponentType.AsTree:
      return new AsTree({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    case ComponentType.AsTextBox:
      return new AsTextBox({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    case ComponentType.Label:
      return new Label({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    case ComponentType.TagInput:
      return new TagInput({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    case ComponentType.AsPanel:
      return new AsPanel({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    case ComponentType.AsDateBox:
      return new AsDateBox({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    case ComponentType.AsCheckBox:
      return new AsCheckBox({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    case ComponentType.GroupBox:
      return new GroupBox({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    case ComponentType.TextArea:
      return new TextArea({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    case ComponentType.AsForm:
      return new AsForm({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    case ComponentType.FormPanel: {
      if (!loadComponent) {
        throw new Error('loadComponent parameter is missing');
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
        reactElement: reactElement,
      });
    }

    case ComponentType.SplitPanel:
      if (!getChildren) {
        throw new Error('getChildren parameter is missing');
      }
      return new SplitPanel({
        id: control.id,
        parent: parent,
        getChildren: getChildren,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    case ComponentType.TabPage:
      if (!getChildren) {
        throw new Error('getChildren parameter is missing');
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
      });

    case ComponentType.TabControl:
      return new TabControl({
        id: control.id,
        parent: parent,
        data: {
          type: componentType,
          identifier: control.name,
        },
        properties: properties,
      });

    default:
      throw new Error(`Unknown component type: ${componentType}`);
  }
}

export async function toComponentRecursive(
  control: IApiControl,
  parent: Component | null,
  allComponents: Component[],
  getChildren?: (component: Component) => Component[],
  loadComponent?: (componentId: string) => Promise<ReactElement>,
): Promise<Component[]> {
  const newComponent = await controlToComponent(control, parent, getChildren, loadComponent);
  for (const childControl of control.children) {
    await toComponentRecursive(
      childControl,
      newComponent,
      allComponents,
      getChildren,
      loadComponent,
    );
  }
  allComponents.push(newComponent);
  return allComponents;
}
