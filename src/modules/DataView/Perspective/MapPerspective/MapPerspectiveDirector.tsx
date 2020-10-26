import React from "react";
import { action, flow } from "mobx";
import { IDataViewBodyUI, IDataViewToolbarUI } from "modules/DataView/DataViewUI";
import { TypeSymbol } from "dic/Container";
import { SectionViewSwitchers } from "modules/DataView/DataViewTypes";
import { getIdent, IIId } from "utils/common";
import { DataViewHeaderAction } from "gui02/components/DataViewHeader/DataViewHeaderAction";
import { Icon } from "gui02/components/Icon/Icon";
import { IMapPerspective } from "./MapPerspective";
import { Observer } from "mobx-react";
import { IPerspective } from "../Perspective";
import { MapPerspectiveCom } from "./MapPerspectiveUI";
import { MapPerspectiveSetup } from "./MapPerspectiveSetup";
import { MapSourceData } from "./MapSourceData";

export class MapPerspectiveDirector implements IIId {
  $iid = getIdent();

  constructor(
    public dataViewToolbarUI = IDataViewToolbarUI(),
    public dataViewBodyUI = IDataViewBodyUI(),
    public mapPerspective = IMapPerspective(),
    public perspective = IPerspective()
  ) {}

  mapPerspectiveSetup: MapPerspectiveSetup = null!;
  mapSourceData: MapSourceData = null!;

  @action.bound
  setup() {
    this.dataViewBodyUI.contrib.put({
      $iid: this.$iid,
      render: () => (
        <Observer key={this.$iid}>
          {() => (
            <MapPerspectiveCom
              mapCenter={this.mapPerspectiveSetup.mapCenter || { lat: 0, lng: 0 }}
              mapSourceData={this.mapSourceData}
              isReadOnly={this.mapPerspectiveSetup.isReadOnlyView}
              isActive={this.mapPerspective.isActive}
              onChange={(geoJson) => {
                console.log("Change: ", geoJson);
                this.mapSourceData.handleGeometryChange(geoJson);
              }}
            />
          )}
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
