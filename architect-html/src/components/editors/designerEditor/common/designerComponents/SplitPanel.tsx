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

import { screenLayer } from 'src/components/editors/designerEditor/common/Layers.ts';
import { ReactElement } from 'react';
import S from 'src/components/editors/designerEditor/common/designerComponents/Components.module.scss';
import { Component } from 'src/components/editors/designerEditor/common/designerComponents/Component.tsx';
import { IComponentData } from 'src/components/editors/designerEditor/common/ComponentType.tsx';
import { EditorProperty } from 'src/components/editors/gridEditor/EditorProperty.ts';
import { IApiEditorProperty } from 'src/API/IArchitectApi.ts';

const childGap = 10;

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

  update() {
    const children = this.getChildren(this);
    if (children.length == 0) {
      return;
    }
    if (children.length > 2) {
      throw new Error('Split panel cannot have more than 2 children');
    }
    const orientation = this.getOrientation();
    switch (orientation) {
      case Orientation.Horizontal: {
        const { upperChild, lowerChild } = this.getUpperAndLowerChild(children);
        upperChild.relativeTop = childGap;
        upperChild.relativeLeft = childGap;
        upperChild.width = Math.round(this.width - childGap * 2);
        upperChild.height = Math.round(this.height / 2 - childGap * 1.5);
        if (lowerChild) {
          lowerChild.relativeTop = Math.round(this.height / 2 + childGap * 0.5);
          lowerChild.relativeLeft = childGap;
          lowerChild.width = Math.round(this.width - childGap * 2);
          lowerChild.height = Math.round(this.height / 2 - childGap * 1.5);
        }
        break;
      }
      case Orientation.Vertical: {
        const { leftChild, rightChild } = this.getLeftAndRightChild(children);
        leftChild.relativeLeft = childGap;
        leftChild.relativeTop = childGap;
        leftChild.width = Math.round(this.width / 2 - childGap * 1.5);
        leftChild.height = Math.round(this.height - childGap * 2);
        if (rightChild) {
          rightChild.relativeTop = childGap;
          rightChild.relativeLeft = Math.round(this.width / 2 + childGap * 0.5);
          rightChild.width = Math.round(this.width / 2 - childGap * 1.5);
          rightChild.height = Math.round(this.height - childGap * 2);
        }
        break;
      }
      default:
        throw new Error(`Unknown split panel orientation "${orientation}"`);
    }
  }

  private getOrientation() {
    const propertyValue = this.get('Orientation');
    switch (propertyValue) {
      case '0':
      case 'Horizontal':
        return Orientation.Horizontal;
      case '1':
      case 'Vertical':
        return Orientation.Vertical;
      default:
        return propertyValue;
    }
  }

  getUpperAndLowerChild(children: Component[]) {
    if (children.length == 1) {
      return {
        upperChild: children[0],
        lowerChild: undefined,
      };
    }
    if (children[0].relativeTop < children[1].relativeTop) {
      return {
        upperChild: children[0],
        lowerChild: children[1],
      };
    }
    return {
      upperChild: children[1],
      lowerChild: children[0],
    };
  }

  getLeftAndRightChild(children: Component[]) {
    if (children.length == 1) {
      return {
        leftChild: children[0],
        rightChild: undefined,
      };
    }
    if (children[0].relativeLeft > children[1].relativeLeft) {
      return {
        leftChild: children[0],
        rightChild: children[1],
      };
    }
    return {
      leftChild: children[1],
      rightChild: children[0],
    };
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

  set value(value: any) {
    super.value = value;
    this.splitPanel.update();
  }
}

enum Orientation {
  Horizontal,
  Vertical,
}
