import React from "react";
import { observer, Observer } from "mobx-react";
import { TabbedViewPanel } from "gui/Components/TabbedView/TabbedViewPanel";
import { TabbedViewHandleRow } from "gui/Components/TabbedView/TabbedViewHandleRow";
import { TabbedViewHandle } from "gui/Components/TabbedView/TabbedViewHandle";
import { action, observable } from "mobx";
import { TabbedView } from "gui/Components/TabbedView/TabbedView";
import { findUIChildren } from "xmlInterpreters/screenXml";
import { TabbedViewPanelsContainer } from "gui/Components/TabbedView/TabbedViewPanelsContainer";
import { IDataView } from "model/entities/types/IDataView";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { findStopping } from "xmlInterpreters/xmlUtils";

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
    if(firstGridId) {
      const dataView = this.props.dataViewMap.get(firstGridId);
      const tablePanelView = getTablePanelView(dataView);
      tablePanelView.triggerOnFocusTable();
    }
  }

  getFirstGridId(box: any){
    return findStopping(box, (node) => node?.attributes?.Type === "Grid")
      .map(element => element.attributes["Id"])
      .find(element => element)
  }

  render() {
    const { boxes } = this.props;
    return (
      <TabbedView>
        <TabbedViewHandleRow>
          {boxes.map((box) => (
            <Observer key={box.$iid}>
              {() => (
                <TabbedViewHandle
                  title={box.attributes.Name}
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
