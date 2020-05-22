import React from "react";
import { observer, Observer } from "mobx-react";
import { TabbedViewPanel } from "gui02/components/TabbedView/TabbedViewPanel";
import { TabbedViewHandleRow } from "gui02/components/TabbedView/TabbedViewHandleRow";
import { TabbedViewHandle } from "gui02/components/TabbedView/TabbedViewHandle";
import { observable, action } from "mobx";
import { TabbedView } from "gui02/components/TabbedView/TabbedView";
import { findUIChildren } from "xmlInterpreters/screenXml";
import { TabbedViewPanelsContainer } from "gui02/components/TabbedView/TabbedViewPanelsContainer";

@observer
export class CScreenSectionTabbedView extends React.Component<{
  boxes: any[];
  nextNode: (node: any) => React.ReactNode;
}> {
  @observable activePanelId: string =
    this.props.boxes.length > 0 ? this.props.boxes[0].attributes.Id : "";

  @action.bound activateTab(tabId: string) {
    this.activePanelId = tabId;
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
