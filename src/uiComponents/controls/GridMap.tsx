import * as React from "react";
import { GridViewType } from "src/Grid/types";
import { observer, inject, Observer } from "mobx-react";
import { observable, when, computed, action } from "mobx";
import { AutoSizer } from "react-virtualized";

@inject("gridPaneBacking")
@observer
export class GridMap extends React.Component<any> {
  constructor(props: any) {
    super(props);
  }

  @observable
  private isAfterMountDeath = false;

  @computed
  public get isActiveView() {
    const { gridPaneBacking } = this.props;
    const { gridInteractionSelectors } = gridPaneBacking;
    return gridInteractionSelectors.activeView === GridViewType.Map;
  }

  @action.bound
  private handleResize({
    width,
    height
  }: {
    width: number;
    height: number;
  }): void {
    // When on hidden part of ui first it would not properly catch its dimensions.
    // => Mount the map only after its domensions are known.
    if (width > 100 && height > 100) {
      this.isAfterMountDeath = true;
    }
  }

  public render() {
    const { gridPaneBacking } = this.props;
    const { gridInteractionSelectors } = gridPaneBacking;
    const { isActiveView } = this;
    return (
      <div
        className="oui-mapview"
        style={{ display: !isActiveView ? "none" : undefined }}
      >
        <AutoSizer onResize={this.handleResize}>
          {({ width, height }) => (
            <Observer>
              {() =>
                isActiveView &&
                this.isAfterMountDeath && (
                  <iframe
                    style={{
                      width,
                      height: height-3,
                      boxSizing: "border-box",
                      border: "none"
                    }}
                    scrolling="no"
                    src="https://www.openstreetmap.org/export/embed.html?bbox=10.206298828125002,48.22101291025667,19.379882812500004,51.36149165915505&layer=mapnik"
                  />
                )
              }
            </Observer>
          )}
        </AutoSizer>
      </div>
    );
  }
}
