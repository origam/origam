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

import { RootStoreContext } from '@/main.tsx';
import { ComponentType, IComponentData } from '@editors/designerEditor/common/ComponentType.tsx';
import { Component } from '@editors/designerEditor/common/designerComponents/Component.tsx';
import S from '@editors/designerEditor/common/designerComponents/Components.module.scss';
import { screenLayer } from '@editors/designerEditor/common/Layers.ts';
import { DesignerStateContext } from '@editors/designerEditor/screenEditor/ScreenEditor.tsx';
import { EditorProperty } from '@editors/gridEditor/EditorProperty.ts';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler.ts';
import { action, observable } from 'mobx';
import { Observer, observer } from 'mobx-react-lite';
import { ReactElement, useContext } from 'react';
import { TriggerEvent, useContextMenu } from 'react-contexify';

export class TabControl extends Component {
  @observable private accessor tabs: TabPage[] = [];

  get zIndex(): number {
    return this.countParents() + screenLayer;
  }

  get numberOfTabs() {
    return this.tabs.length;
  }

  constructor(args: {
    id: string;
    parent: Component | null;
    data: IComponentData;
    properties: EditorProperty[];
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

  // The TapPage children derive their position from the TapPage's position.
  // The TabPage itself cannot be moved but its position has to be kept in
  // sync with the TabControl's position
  set absoluteLeft(value: number) {
    super.absoluteLeft = value;
    for (const tab of this.tabs) {
      tab.absoluteLeft = value;
    }
  }

  get absoluteLeft() {
    return super.absoluteLeft;
  }

  set absoluteTop(value: number) {
    super.absoluteTop = value;
    for (const tab of this.tabs) {
      tab.absoluteTop = value;
    }
  }
  get absoluteTop() {
    return super.absoluteTop;
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
                .sort((a, b) => a.get('Text').localeCompare(b.get('Text')))
                .map(tab => (
                  <TabLabel key={tab.id} onClick={() => this.setVisible(tab.id)} tabPage={tab} />
                ))}
            </div>
          </div>
        )}
      </Observer>
    );
  }
}

const TabLabel = observer(({ tabPage, onClick }: { tabPage: TabPage; onClick: () => void }) => {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const designerState = useContext(DesignerStateContext);

  const { show } = useContextMenu({
    id: 'TAB_LABEL_MENU',
  });

  function handleContextMenu(event: TriggerEvent) {
    event.preventDefault();
    show({
      event,
      props: {
        tabId: tabPage.id,
        rootStore: rootStore,
        deleteDisabled: (tabPage.parent as TabControl).numberOfTabs === 1,
        onDelete: () => {
          if (designerState) {
            run({ generator: designerState.delete(tabPage.getAllChildren()) });
          }
        },
        onAdd: () => {
          if (designerState) {
            run({ generator: designerState.createTabPage(tabPage.parent! as TabControl) });
          }
        },
      },
    });
  }

  return (
    <>
      <div
        className={tabPage.isActive ? S.activeTab : ''}
        onClick={event => {
          (event as any).clickedComponent = tabPage;
          onClick();
        }}
        onContextMenu={handleContextMenu}
      >
        {tabPage.get('Text')}
      </div>
    </>
  );
});

export class TabPage extends Component {
  @observable accessor hideChildren: boolean = false;
  @observable private accessor _isActive: boolean = false;
  private getChildren: (component: Component) => Component[];

  constructor(args: {
    id: string;
    parent: TabControl;
    data: IComponentData;
    properties: EditorProperty[];
    getChildren: (component: Component) => Component[];
  }) {
    super(args);
    this.getChildren = args.getChildren;

    if (!args.parent || args.parent.data.type !== ComponentType.TabControl) {
      throw new Error('Parent of TabPage must be a TabControl');
    }
    (args.parent as TabControl).registerTab(this);

    // TabPages' dimension properties always come with default values from the server.
    // That is ok. They have to be the same size and ath the same location as
    // the parent TabControl anyway.
    this.absoluteLeft = this.parent!.absoluteLeft;
    this.absoluteTop = this.parent!.absoluteTop;
    this.width = this.parent!.width;
    this.height = this.parent!.height;
  }

  get isActive(): boolean {
    return !this.hideChildren && this._isActive;
  }

  get childOffsetLeft() {
    return 5;
  }

  get childOffsetTop() {
    return 20;
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
