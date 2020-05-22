import React from "react";
import { action, flow } from "mobx";
import { IDataViewToolbarUI, IDataViewBodyUI } from "modules/DataView/DataViewUI";
import { TypeSymbol } from "dic/Container";
import { SectionViewSwitchers } from "modules/DataView/DataViewTypes";
import { IIId, getIdent } from "utils/common";
import { DataViewHeaderAction } from "gui02/components/DataViewHeader/DataViewHeaderAction";
import { Icon } from "gui02/components/Icon/Icon";
import { IMapPerspective } from "./MapPerspective";
import { Observer } from "mobx-react";
import { IPerspective } from "../Perspective";
import { MapPerspectiveCom } from "./MapPerspectiveUI";

export class MapPerspectiveDirector implements IIId {
  $iid = getIdent();

  constructor(
    public dataViewToolbarUI = IDataViewToolbarUI(),
    public dataViewBodyUI = IDataViewBodyUI(),
    public mapPerspective = IMapPerspective(),
    public perspective = IPerspective()
  ) {}

  @action.bound
  setup() {
    this.dataViewBodyUI.contrib.put({
      $iid: this.$iid,
      render: () => (
        <Observer key={this.$iid}>
          {() => (!this.mapPerspective.isActive ? <></> : <MapPerspectiveCom />)}
        </Observer>
      ),
    });

    this.dataViewToolbarUI.contrib.put({
      $iid: this.$iid,
      section: SectionViewSwitchers,
      render: () => (
        <Observer key={this.$iid}>
          {() => (
            <DataViewHeaderAction
              onClick={flow(this.mapPerspective.handleToolbarBtnClick)}
              isActive={this.mapPerspective.isActive}
            >
              <Icon src="./icons/geo-coordinates.svg" />
            </DataViewHeaderAction>
          )}
        </Observer>
      ),
    });

    this.perspective.contrib.put(this.mapPerspective);
  }

  @action.bound
  teardown() {
    this.dataViewBodyUI.contrib.del(this);
    this.dataViewToolbarUI.contrib.del(this);
    this.perspective.contrib.del(this.mapPerspective);
  }

  dispose() {
    this.teardown();
  }
}

export const IMapPerspectiveDirector = TypeSymbol<MapPerspectiveDirector>(
  "IMapPerspectiveDirector"
);
