import {
  screenLayer
} from "src/components/editors/designerEditor/common/Layers.ts";
import {
  ComponentType,
  IComponentData
} from "src/components/editors/designerEditor/common/ComponentType.tsx";
import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import { action, observable } from "mobx";
import { ReactElement, useContext } from "react";
import S
  from "src/components/editors/designerEditor/common/designerComponents/Components.module.scss";
import { Observer, observer } from "mobx-react-lite";
import {
  Component,
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";
import { TriggerEvent, useContextMenu } from "react-contexify";
import { RootStoreContext } from "src/main.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import {
  DesignerStateContext
} from "src/components/editors/designerEditor/screenEditor/ScreenEditor.tsx";

export class TabControl extends Component {

  @observable private accessor tabs: TabPage[] = [];

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
    const isActive = this.tabs.length === 0; // i.e.  the first registered tab will be active
    tab.initializeVisibility(isActive, isActive ? this.hideChildren : true); // We need to recursively hide children of inactive tabs
    this.tabs.push(tab);
  }

  @action
  setVisible(tabId: string) {
    for (const tab of this.tabs) {
      tab.isActive = tab.id === tabId;
    }
  }

  get canHaveChildren(): boolean {
    return false;
  }

  get childOffsetLeft() {
    return 5;
  }

  get childOffsetTop() {
    return 20;
  }

  getDesignerRepresentation(): ReactElement | null {
    return (
      <Observer>
        {() => (
          <div className={S.tabPageContainer}>
            <div className={S.tabs}>
              {this.tabs
                .slice()
                .sort((a, b) => a.get("Text").localeCompare(b.get("Text")))
                .map(tab =>
                  <TabLabel
                    onClick={() => this.setVisible(tab.id)}
                    tabPage={tab}
                  />
                )
              }
            </div>
            {/*<div className={S.designSurfaceInput}></div>*/}
          </div>
        )}
      </Observer>
    );
  }
}

const TabLabel = observer((
  {
    tabPage,
    onClick,
  }: {
    tabPage: TabPage;
    onClick: () => void;
  }) => {

  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const designerState = useContext(DesignerStateContext);

  const {show} = useContextMenu({
    id: "TAB_LABEL_MENU"
  });

  function handleContextMenu(event: TriggerEvent) {
    event.preventDefault();
    show({
      event,
      props: {
        tabId: tabPage.id,
        rootStore: rootStore,
        onDelete: () => {
          if (designerState) {
            run({generator: designerState.delete(tabPage.getAllChildren())})
          }
        },
        onAdd: () => {
          if (designerState) {
            run({generator: designerState.createTabPage(tabPage.parent! as TabControl)})
          }
        }
      }
    });
  }

  return (
    <>
      <div
        className={tabPage.isActive ? S.activeTab : ""}
        onClick={event => {
          (event as any).clickedComponent = tabPage;
          onClick();
        }}
        onContextMenu={handleContextMenu}
      >
        {tabPage.get("Text")}
      </div>
    </>
  );
});

export class TabPage extends Component {

  @observable accessor hideChildren: boolean = false;
  @observable private accessor _isActive: boolean = false;
  private getChildren: (component: Component) => Component[];

  constructor(args: {
    id: string,
    parent: TabControl,
    data: IComponentData,
    properties: EditorProperty[],
    getChildren: (component: Component) => Component[]
  }) {
    super(args);
    this.getChildren = args.getChildren;

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
    return !this.hideChildren && this._isActive;
  }

  set isActive(value: boolean) {
    this._isActive = value;
    const hideChildren = !this._isActive;
    this.showHideChildrenRecursive(this, hideChildren);
  }

  initializeVisibility(isActive: boolean, hideChildren: boolean) {
    this._isActive = isActive;
    this.hideChildren = hideChildren;
  }

  showHideChildrenRecursive(component: Component, hideChildren: boolean) {
    component.hideChildren = hideChildren;
    const children = this.getChildren(component);
    for (const child of children) {
      this.showHideChildrenRecursive(child, hideChildren);
    }
  }

  getAllChildren(component?: Component, otherChildren?: Component[]) {
    const currentComponent = component ?? this;
    let allChildren = otherChildren ?? [];
    allChildren.push(currentComponent);
    const currentComponentChildren = this.getChildren(currentComponent);
    for (const child of currentComponentChildren) {
      allChildren = [...this.getAllChildren(child, allChildren)];
    }
    return allChildren;
  }

  isPointInside(x: number, y: number) {
    // Delegating to the parent - TabControl solves the case when the point
    // is in the tab label area. This method would return false (which is wrong)
    // if not overridden.
    return this.parent!.isPointInside(x, y);
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
