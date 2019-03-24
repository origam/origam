import * as React from "react";
import { observer, inject } from "mobx-react";
import { ITabPanelProps, ITabsProps } from './types';
import { ITabs, ITabPanel } from "src/presenter/types/IScreenPresenter";




@observer
export class TabPanel extends React.Component<ITabPanelProps> {
  render() {
    return (
      <div
        className={
          "tabs-body" +
          (this.props.activePanelId !== this.props.id ? " hidden" : "")
        }
      >
        {this.props.children}
      </div>
    );
  }
}

@inject(
  (
    { tabPanelsMap }: { tabPanelsMap: Map<string, ITabs> },
    props: ITabsProps
  ) => {
    return {
      controller: tabPanelsMap.get(props.id)
    };
  }
)
@observer
export class TabbedPanel extends React.Component<ITabsProps> {
  render() {
    return (
      <div className="tabs">
        <div className="tabs-handles">
          {this.props.panels.map((panel: ITabPanel) => (
            <div
              className={
                "tabs-handle" +
                (this.props.controller &&
                this.props.controller.activeTabId === panel.id
                  ? " active"
                  : "")
              }
              key={panel.id}
              onClick={(event: any) =>
                this.props.controller &&
                this.props.controller.onHandleClick &&
                this.props.controller.onHandleClick(event, panel.id)
              }
            >
              {panel.label}
            </div>
          ))}
        </div>
        {this.props.panels.map((panel: ITabPanel) => (
          <TabPanel
            id={panel.id}
            key={panel.id}
            activePanelId={
              (this.props.controller && this.props.controller.activeTabId) || ""
            }
          >
            {panel.content}
          </TabPanel>
        ))}
      </div>
    );
  }
}