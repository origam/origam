import {
  ComponentType,
  IComponentData
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import { action, observable } from "mobx";

import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  controlLayer,
  screenLayer,
  sectionLayer
} from "src/components/editors/designerEditor/common/Layers.ts";
import {
  LabelPosition,
  parseLabelPosition
} from "src/components/editors/designerEditor/common/LabelPosition.tsx";
import S
  from "src/components/editors/designerEditor/common/designerComponents/Components.module.scss";
import { ReactElement } from "react";
import { Observer } from "mobx-react-lite";

export abstract class Component {
  id: string;
  @observable.ref accessor parent: Component | null;
  data: IComponentData;
  @observable accessor properties: EditorProperty[];
  @observable.ref private accessor _designerRepresentation: ReactElement | null = null;
  accessor hideChildren = false;

  get designerRepresentation(): ReactElement | null {
    if(this.parent?.hideChildren){
      return null;
    }
    if (!this._designerRepresentation) {
      action(()=>{
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
    return this.relativeLeft + (this.parent?.absoluteLeft ?? 0);
  }

  set absoluteLeft(value: number) {
    this.relativeLeft = value - (this.parent?.absoluteLeft ?? 0);
  }

  get absoluteRight(): number {
    return this.absoluteLeft + this.width;
  }

  get absoluteTop(): number {
    return this.relativeTop + (this.parent?.absoluteTop ?? 0);
  }

  set absoluteTop(value: number) {
    this.relativeTop = value - (this.parent?.absoluteTop ?? 0);
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
    return true;
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

export class SplitPanel extends Component {
  get canHaveChildren(): boolean {
    return true;
  }

  get zIndex(): number {
    return this.countParents() + screenLayer;
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

export class AsForm extends Component {
  get canHaveChildren(): boolean {
    return true;
  }

  get zIndex(): number {
    return screenLayer;
  }

  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.panel}>
      </div>
    );
  }
}

export class AsPanel extends Component {
  get canHaveChildren(): boolean {
    return true;
  }

  get zIndex(): number {
    return sectionLayer;
  }

  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.panel}>
      </div>
    );
  }
}

export class AsCheckBox extends Component {
  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.designSurfaceEditorContainer}>
        <div className={S.designSurfaceCheckbox}></div>
        <div>{this.properties.find(x=>x.name === "Text")?.value}</div>
      </div>
    );
  }
}

export class FormPanel extends Component {

  reactElement: ReactElement;

  constructor(args: {
    id: string,
    parent: Component | null,
    data: IComponentData,
    properties: EditorProperty[],
    reactElement: ReactElement,
  }){
    super(args);
    this.reactElement = args.reactElement;
  }

  getDesignerRepresentation(): ReactElement | null {
    return this.reactElement;
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

export class TabControl extends Component {

  private tabs: TabPage[] = [];

  get zIndex(): number {
    return this.countParents() + screenLayer;
  }

  constructor(args: {
    id: string,
    parent: Component | null,
    data: IComponentData,
    properties: EditorProperty[],
  }) {
    super(args);
  }

  registerTab(tab: TabPage) {
    if (this.tabs.length === 0) {
      tab.hideChildren = true;
    }
    this.tabs.push(tab);
  }

  @action
  setVisible(tabId: string){
    for (const tab of this.tabs){
      tab.hideChildren = tab.id !== tabId;
    }
  }

  get canHaveChildren(): boolean {
    return false;
  }

  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.tabPageContainer}>
        <div className={S.tabs}>
          {this.tabs.map(tab =>
            <Observer key={tab.id}>
              {() => (
                <div
                  className={tab.isActive ? S.activeTab : ""}
                  onClick={() => this.setVisible(tab.id)}
                >
                  {tab.get("Text")}
                </div>
              )}
            </Observer>)
          }
        </div>
        {/*<div className={S.designSurfaceInput}></div>*/}
      </div>
    );
  }
}

export class TabPage extends Component {

  @observable accessor hideChildren: boolean = false;

  constructor(args: {
    id: string,
    parent: TabControl,
    data: IComponentData,
    properties: EditorProperty[],
  }) {
    super(args);

    if (!args.parent || args.parent.data.type !== ComponentType.TabControl) {
      throw new Error("Parent of TabPage must be a TabControl");
    }
    (args.parent as TabControl).registerTab(this);

    // TabPages' width and height properties always come with default values from the server.
    // That is ok. They have to be the same size as the parent TabControl anyway.
    const parentWidth = this.parent!.properties!.find(x => x.name == "Width")!.value;
    const widthProperty = this.properties.find(x => x.name == "Width")!
    widthProperty.value = parentWidth;

    const parentHeight = this.parent!.properties!.find(x => x.name == "Height")!.value;
    const heightProperty = this.properties.find(x => x.name == "Height")!
    heightProperty.value = parentHeight;
  }

  get isActive(): boolean {
    return !this.hideChildren;
  }

  get canHaveChildren(): boolean {
    return true;
  }

  get zIndex(): number {
    return this.countParents() + screenLayer;
  }

  getDesignerRepresentation(): ReactElement | null {
    return null;
  }
}
