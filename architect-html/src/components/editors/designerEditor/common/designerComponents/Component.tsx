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

import {
  IComponentData
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import { action, observable } from "mobx";

import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  controlLayer,
  sectionLayer
} from "src/components/editors/designerEditor/common/Layers.ts";
import {
  LabelPosition,
  parseLabelPosition
} from "src/components/editors/designerEditor/common/LabelPosition.tsx";
import S
  from "src/components/editors/designerEditor/common/designerComponents/Components.module.scss";
import { ReactElement } from "react";

export abstract class Component {
  id: string;
  @observable.ref accessor parent: Component | null;
  data: IComponentData;
  @observable accessor properties: EditorProperty[];
  @observable.ref private accessor _designerRepresentation: ReactElement | null = null;
  accessor hideChildren = false;

  get designerRepresentation(): ReactElement | null {
    if (this.parent && !this.parent.isActive) {
      return null;
    }
    if (!this._designerRepresentation) {
      action(() => {
        this._designerRepresentation = this.getDesignerRepresentation()
      })();
    }
    return this._designerRepresentation;
  }

  set designerRepresentation(value: ReactElement | null) {
    this._designerRepresentation = value;
  }

  get relativeLeft(): number {
    return this.getProperty("Left")!.value;
  }

  set relativeLeft(value: number) {
    this.getProperty("Left")!.value = value;
  }

  get relativeTop(): number {
    return this.getProperty("Top")!.value;
  }

  set relativeTop(value: number) {
    this.getProperty("Top")!.value = value;
  }

  get absoluteLeft(): number {
    return this.relativeLeft
      + (this.parent?.absoluteLeft ?? 0)
      + (this.parent?.childOffsetLeft ?? 0);
  }

  set absoluteLeft(value: number) {
    this.relativeLeft = value
      - (this.parent?.absoluteLeft ?? 0)
      - (this.parent?.childOffsetLeft ?? 0);
  }

  get absoluteRight(): number {
    return this.absoluteLeft + this.width;
  }

  get absoluteTop(): number {
    return this.relativeTop
      + (this.parent?.absoluteTop ?? 0)
      + (this.parent?.childOffsetTop ?? 0);
  }

  set absoluteTop(value: number) {
    this.relativeTop = value
      - (this.parent?.absoluteTop ?? 0)
      - (this.parent?.childOffsetTop ?? 0);
  }

  get childOffsetLeft() {
    return 0;
  }

  get childOffsetTop() {
    return 0;
  }

  get absoluteBottom(): number {
    return this.absoluteTop + this.height;
  }

  get width(): number {
    return this.getProperty("Width")!.value;
  }

  set width(value: number) {
    this.getProperty("Width")!.value = value;
  }

  get height(): number {
    return this.getProperty("Height")!.value;
  }

  set height(value: number) {
    this.getProperty("Height")!.value = value;
  }

  get labelWidth(): number {
    return this.getProperty("CaptionLength")!.value;
  }

  set labelWidth(value: number) {
    this.getProperty("CaptionLength")!.value = value;
  }

  private _labelPosition: LabelPosition;
  get labelPosition(): LabelPosition {
    return this._labelPosition;
  }

  set labelPosition(value: number) {
    this._labelPosition = value;
  }

  get zIndex(): number {
    return controlLayer;
  }

  get isActive(): boolean {
    return !this.hideChildren;
  }

  constructor(args: {
    id: string,
    parent: Component | null,
    data: IComponentData,
    properties: EditorProperty[],
  }) {
    this.id = args.id;
    this.data = args.data;
    this.properties = args.properties;
    this._labelPosition = parseLabelPosition(this.get("CaptionPosition"));
    this.parent = args.parent;
    if(this.parent){
      this.hideChildren = this.parent.hideChildren;
    }
  }

  isPointInside(x: number, y: number) {
    return (
      x >= this.absoluteLeft &&
      x <= this.absoluteLeft + this.width &&
      y >= this.absoluteTop &&
      y <= this.absoluteTop + this.height
    );
  }

  countParents(): number {
    if (!this.parent) {
      return 0;
    }
    return this.parent.countParents() + 1;
  }

  getLabelStyle() {
    switch (this.labelPosition) {
      case LabelPosition.Left:
        return {
          left: `${this.absoluteLeft - this.labelWidth}px`,
          top: `${this.absoluteTop}px`,
          width: `${this.labelWidth}px`,
          height: `${this.height}px`,
        }
      case LabelPosition.Right:
        return {
          left: `${this.absoluteLeft + this.width}px`,
          top: `${this.absoluteTop}px`,
          width: `${this.labelWidth}px`,
          height: `${this.height}px`,
        }
      case LabelPosition.Bottom:
        return {
          left: `${this.absoluteLeft}px`,
          top: `${this.absoluteTop + this.height}px`,
          width: `${this.width}px`,
          height: `${this.labelWidth}px`,
        }
      case LabelPosition.Top:
        return {
          left: `${this.absoluteLeft}px`,
          top: `${this.absoluteTop + -20}px`,
          width: `${this.width}px`,
          height: `${this.labelWidth}px`,
        }
      case null:
      case undefined:
      case LabelPosition.None:
        return {display: 'none'}
    }
  }

  get(text: string): any {
    return this.getProperty(text)?.value;
  }

  getProperty(name: string): EditorProperty | undefined {
    return this.properties.find(p => p.name === name);
  }

  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.designSurfaceEditorContainer}>
        <div className={S.designSurfaceInput}></div>
      </div>
    );
  }

  get canHaveChildren(): boolean {
    return false;
  }

  update() {
  }
}


export class GroupBox extends Component {
  get canHaveChildren(): boolean {
    return true;
  }

  get zIndex(): number {
    return this.countParents() + sectionLayer;
  }

  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.groupBoxContent}>
        <div
          className={S.groupBoxHeader}>{this.properties.find(x => x.name === "Text")?.value}
        </div>
      </div>
    );
  }
}

export class AsCombo extends Component {
}

export class AsTextBox extends Component {
}

export class TagInput extends Component {
}

export class AsDateBox extends Component {
}

export class TextArea extends Component {
}

export class AsTree extends Component {
}

