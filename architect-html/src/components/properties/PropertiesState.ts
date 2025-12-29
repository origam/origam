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

import { IUpdatePropertiesResult } from '@api/IArchitectApi';
import { EditorProperty } from '@editors/gridEditor/EditorProperty';
import { IPropertyManager } from '@editors/propertyEditor/IPropertyManager';
import { observable } from 'mobx';

import { IComponentProvider } from '@editors/designerEditor/common/IComponentProvider.tsx';
import { Component } from '@editors/designerEditor/common/designerComponents/Component.tsx';

export class PropertiesState implements IPropertyManager {
  @observable private accessor provider: IComponentProvider | undefined;

  setComponentProvider(provider: IComponentProvider | undefined) {
    if (this.provider !== provider) {
      this.provider = provider;
    }
  }

  get selectedComponent() {
    return this.provider?.selectedComponent;
  }

  get properties() {
    return this.selectedComponent?.properties ?? [];
  }

  get components() {
    return this.provider?.components ?? [];
  }

  getComponentLabel(component: Component | undefined) {
    if (!component) {
      return '';
    }
    const name = component?.data.identifier ?? component?.getProperty('Text')?.value ?? '';
    const type = (component?.data.type ?? '').replace('Origam.Gui.Win.', '');
    return `${name} [${type}]`;
  }

  setSelectedComponent(id: string | undefined): void {
    if (this.provider) {
      this.provider.selectedComponent = this.provider?.components.find(x => x.id === id) ?? null;
    }
  }

  onPropertyUpdated(
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    _property: EditorProperty,
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    _value: any,
  ): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult> {
    throw new Error('Method not implemented.');
  }
}
