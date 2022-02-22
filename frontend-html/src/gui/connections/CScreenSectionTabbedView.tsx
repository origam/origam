/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

import React from "react";
import { observer, Observer } from "mobx-react";
import { TabbedViewPanel } from "gui/Components/TabbedView/TabbedViewPanel";
import { TabbedViewHandleRow } from "gui/Components/TabbedView/TabbedViewHandleRow";
import { TabbedViewHandle } from "gui/Components/TabbedView/TabbedViewHandle";
import { action, observable } from "mobx";
import { TabbedView } from "gui/Components/TabbedView/TabbedView";
import { TabbedViewPanelsContainer } from "gui/Components/TabbedView/TabbedViewPanelsContainer";
import { IDataView } from "model/entities/types/IDataView";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { findStopping, findUIChildren } from "xmlInterpreters/xmlUtils";

@observer
export class CScreenSectionTabbedView extends React.Component<{
  boxes: any[];
  nextNode: (node: any) => React.ReactNode;
  dataViewMap: Map<string, IDataView>
}> {
  @observable activePanelId: string =
    this.props.boxes.length > 0 ? this.props.boxes[0].attributes.Id : "";

  @action.bound activateTab(tabId: string) {
    this.activePanelId = tabId;
    const activeBox = this.props.boxes.find(box => box.attributes["Id"] === tabId);
    const firstGridId = this.getFirstGridId(activeBox)
    if (firstGridId) {
      const dataView = this.props.dataViewMap.get(firstGridId);
      const tablePanelView = getTablePanelView(dataView);
      tablePanelView.triggerOnFocusTable();
    }
  }

  getFirstGridId(box: any) {
    return findStopping(box, (node) => node?.attributes?.Type === "Grid")
      .map(element => element.attributes["Id"])
      .find(element => element)
  }

  render() {
    const {boxes} = this.props;
    return (
      <TabbedView>
        <TabbedViewHandleRow>
          {boxes.map((box) => (
            <Observer key={box.$iid}>
              {() => (
                <TabbedViewHandle
                  title={box.attributes.Name}
                  id={"tabHandle_"+box.attributes.ModelInstanceId}
                  key={box.attributes.Id}
                  isActive={this.activePanelId === box.attributes.Id}
                  onClick={() => this.activateTab(box.attributes.Id)}
                >
                  {box.attributes.Name}
                </TabbedViewHandle>
              )}
            </Observer>
          ))}
        </TabbedViewHandleRow>
        <TabbedViewPanelsContainer>
          {boxes.map((box) => (
            <Observer key={box.$iid}>
              {() => (
                <TabbedViewPanel
                  key={box.attributes.Id}
                  isActive={this.activePanelId === box.attributes.Id}
                >
                  {findUIChildren(box).map((child) => this.props.nextNode(child))}
                </TabbedViewPanel>
              )}
            </Observer>
          ))}
        </TabbedViewPanelsContainer>
      </TabbedView>
    );
  }
}
