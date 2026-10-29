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

import { IApiEditorProperty, PropertyValue } from '@api/IArchitectApi';
import { IComponentData } from '@editors/designerEditor/common/ComponentType';
import { Component } from '@editors/designerEditor/common/designerComponents/Component';
import S from '@editors/designerEditor/common/designerComponents/Components.module.scss';
import { screenLayer } from '@editors/designerEditor/common/Layers';
import { EditorProperty } from '@editors/gridEditor/EditorProperty';
import { ReactElement } from 'react';

const childGap = 10;
const minChildSize = 20;

function limitFirstChildSize(size: number, available: number): number {
  return Math.max(minChildSize, Math.min(size, available - childGap - minChildSize));
}

function getTabIndex(child: Component): number {
  return Number(child.get('TabIndex') ?? 0);
}

function setTabIndex(child: Component, tabIndex: number) {
  const tabIndexProperty = child.getProperty('TabIndex');
  if (tabIndexProperty) {
    tabIndexProperty.value = tabIndex;
  }
}

export class SplitPanel extends Component {
  get canHaveChildren(): boolean {
    return true;
  }

  get zIndex(): number {
    return this.countParents() + screenLayer;
  }

  private readonly getChildren: (component: Component) => Component[];

  constructor(args: {
    id: string;
    parent: Component | null;
    data: IComponentData;
    properties: EditorProperty[];
    getChildren: (component: Component) => Component[];
  }) {
    super(args);
    this.getChildren = args.getChildren;

    const originalOrientationProperty = this.getProperty('Orientation')!;
    const index = this.properties.indexOf(originalOrientationProperty);
    this.properties.splice(index, 1);
    const newOrientationProperty = new OrientationProperty(originalOrientationProperty, this);
    this.properties.push(newOrientationProperty);
  }

  canAcceptChild(child?: Component): boolean {
    return this.getChildren(this).filter(existing => existing.id !== child?.id).length < 2;
  }

  update() {
    this.layout(false);
  }

  onChildrenChanged() {
    this.layout(true);
  }

  private layout(splitEvenly: boolean) {
    const orientation = this.getOrientation();
    if (orientation !== Orientation.Horizontal && orientation !== Orientation.Vertical) {
      throw new Error(`Unknown split panel orientation "${orientation}"`);
    }
    const isHorizontal = orientation === Orientation.Horizontal;
    const [firstChild, secondChild] = this.getChildren(this).sort(
      (childA, childB) =>
        (isHorizontal
          ? childA.relativeTop - childB.relativeTop
          : childA.relativeLeft - childB.relativeLeft) || getTabIndex(childA) - getTabIndex(childB),
    );
    if (!firstChild) {
      return;
    }
    const innerWidth = this.width - childGap * 2;
    const innerHeight = this.height - childGap * 2;
    if (splitEvenly && secondChild) {
      firstChild.width = Math.round((innerWidth - childGap) / 2);
      firstChild.height = Math.round((innerHeight - childGap) / 2);
    }
    firstChild.relativeTop = childGap;
    firstChild.relativeLeft = childGap;
    firstChild.width = isHorizontal
      ? innerWidth
      : limitFirstChildSize(firstChild.width, innerWidth);
    firstChild.height = isHorizontal
      ? limitFirstChildSize(firstChild.height, innerHeight)
      : innerHeight;
    setTabIndex(firstChild, 0);
    if (!secondChild) {
      return;
    }
    secondChild.relativeTop = isHorizontal ? firstChild.height + childGap * 2 : childGap;
    secondChild.relativeLeft = isHorizontal ? childGap : firstChild.width + childGap * 2;
    secondChild.width = isHorizontal
      ? innerWidth
      : Math.max(minChildSize, this.width - childGap - secondChild.relativeLeft);
    secondChild.height = isHorizontal
      ? Math.max(minChildSize, this.height - childGap - secondChild.relativeTop)
      : innerHeight;
    setTabIndex(secondChild, 1);
  }

  private getOrientation() {
    const propertyValue = this.get('Orientation');
    switch (propertyValue) {
      case 0:
      case '0':
      case 'Horizontal':
        return Orientation.Horizontal;
      case 1:
      case '1':
      case 'Vertical':
        return Orientation.Vertical;
      default:
        return propertyValue;
    }
  }

  getDesignerRepresentation(): ReactElement | null {
    return <div className={S.groupBoxContent}></div>;
  }
}

class OrientationProperty extends EditorProperty {
  constructor(
    apiProperty: IApiEditorProperty,
    private splitPanel: SplitPanel,
  ) {
    super(apiProperty);
  }

  get value() {
    return super.value;
  }

  set value(value: PropertyValue) {
    super.value = value;
    this.splitPanel.onChildrenChanged();
  }
}

enum Orientation {
  Horizontal,
  Vertical,
}
