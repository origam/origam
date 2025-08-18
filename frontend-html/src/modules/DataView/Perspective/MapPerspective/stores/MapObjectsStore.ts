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

import { action, computed, flow, reaction } from "mobx";
import { parse as wktParse, stringify as wktStringify } from "wkt";
import { getDataSourceFieldIndexByName } from "model/selectors/DataSources/getDataSourceFieldIndexByName";
import { MapRootStore } from "./MapRootStore";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import { onFieldChangeG } from "model/actions-ui/DataView/TableView/onFieldChange";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import _ from "lodash";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { runGeneratorInFlowWithHandler } from "utils/runInFlowWithHandler";

export class MapObjectsStore {
  constructor(private root: MapRootStore) {
  }

  get dataView() {
    return this.root.dataView;
  }

  get setup() {
    return this.root.mapSetupStore;
  }

  get search() {
    return this.root.mapSearchStore;
  }

  get navigationStore() {
    return this.root.mapNavigationStore;
  }

  @computed get fldLocationIndex() {
    return this.setup.mapLocationMember
      ? getDataSourceFieldIndexByName(this.dataView, this.setup.mapLocationMember)
      : undefined;
  }

  @computed get fldNameIndex() {
    return this.setup.mapTextMember
      ? getDataSourceFieldIndexByName(this.dataView, this.setup.mapTextMember)
      : undefined;
  }

  @computed get fldIconIndex() {
    return this.setup.mapIconMember
      ? getDataSourceFieldIndexByName(this.dataView, this.setup.mapIconMember)
      : undefined;
  }

  @computed get fldIconAzimuth() {
    return this.setup.mapAzimuthMember
      ? getDataSourceFieldIndexByName(this.dataView, this.setup.mapAzimuthMember)
      : undefined;
  }

  @computed get fldColorIndex() {
    return this.setup.mapColorMember
      ? getDataSourceFieldIndexByName(this.dataView, this.setup.mapColorMember)
      : undefined;
  }

  @computed get fldIdentifier() {
    return getDataSourceFieldIndexByName(this.dataView, "Id");
  }

  @computed
  get mapObjects() {
    if (this.fldIdentifier === undefined) {
      return [];
    }
    const result: IMapObject[] = [];
    if (this.setup.isReadOnlyView) {
      const tableRows = this.dataView.tableRows;

      for (let row of tableRows) {
        if (_.isArray(row) && this.fldLocationIndex !== undefined && row[this.fldLocationIndex]) {
          const objectGeoJson = wktParse(row[this.fldLocationIndex]);
          if (objectGeoJson)
            result.push({
              ...objectGeoJson,
              id: row[this.fldIdentifier],
              name: this.fldNameIndex !== undefined ? row[this.fldNameIndex] : "",
              icon: this.fldIconIndex !== undefined ? row[this.fldIconIndex] : undefined,
              color: this.fldColorIndex !== undefined ? row[this.fldColorIndex] : undefined,
              azimuth: this.fldIconAzimuth !== undefined ? row[this.fldIconAzimuth] : undefined,
            });
        }
      }
    } else {
      let selectedRow = getSelectedRow(this.dataView);
      if (selectedRow) {
        const row = selectedRow;
        if (_.isArray(row) && this.fldLocationIndex !== undefined && row[this.fldLocationIndex]) {
          let objectGeoJson: any;
          try {
            objectGeoJson = wktParse(row[this.fldLocationIndex]);
          } catch (e) {
            // TODO: Erorr dialog?
            console.error(e); // eslint-disable-line no-console
          }
          if (objectGeoJson) {
            result.push({
              ...objectGeoJson,
              id: row[this.fldIdentifier],
              name: this.fldNameIndex !== undefined ? row[this.fldNameIndex] : "",
              icon: this.fldIconIndex && row[this.fldIconIndex],
              color: this.fldColorIndex !== undefined ? row[this.fldColorIndex] : undefined,
              azimuth: this.fldIconAzimuth && row[this.fldIconAzimuth],
            });
          }
        }
      }
    }
    return result;
  }

  @action.bound
  handleGeometryChange(geoJson: any) {
    const self = this;
    return flow(function*() {
      const property = getDataViewPropertyById(self.dataView, self.setup.mapLocationMember);
      const selectedRow = getSelectedRow(self.dataView);
      if (property && selectedRow) {
        yield*onFieldChangeG(self.dataView)({
          event: undefined,
          row: selectedRow,
          property: property,
          value: geoJson ? wktStringify(geoJson) : null,
        });
        getDataTable(self.dataView).flushFormToTable(selectedRow);
        yield*getFormScreenLifecycle(self.dataView).onFlushData();
      }
    })();
  }

  @action.bound
  handleLayerClick(id: string) {
    const self = this;
    runGeneratorInFlowWithHandler({
      ctx: this.dataView,
      generator: function*(){
        if (self.setup.isReadOnlyView) {
          getDataView(self.dataView).setSelectedRowId(id);
        }
        getTablePanelView(self.dataView).scrollToCurrentRow();
        self.search.selectSearchResultById(id);
        self.navigationStore.highlightSelectedSearchResult();
      }()
    });
  }

  @action.bound
  handleMapMounted() {
    return reaction(
      () => getSelectedRowId(this.dataView),
      (rowId: string | undefined) => {
        if (this.setup.isReadOnlyView && rowId) {
          this.search.selectSearchResultById(rowId);
          this.navigationStore.highlightSelectedSearchResult();
        }
      },
      {fireImmediately: true}
    );
  }
}

export enum IMapObjectType {
  POINT = "Point",
  POLYGON = "Polygon",
  LINESTRING = "LineString",
}

export interface IMapObjectBase {
  id: string;
  name: string;
  icon: string;
  color: number | undefined;
  azimuth: number;
}

export interface IMapPoint extends IMapObjectBase {
  type: IMapObjectType.POINT;
  coordinates: [number, number];
}

export interface IMapPolygon extends IMapObjectBase {
  type: IMapObjectType.POLYGON;
  coordinates: [number, number][][];
}

export interface IMapPolyline extends IMapObjectBase {
  type: IMapObjectType.LINESTRING;
  coordinates: [number, number][][];
}

export type IMapObject = IMapPoint | IMapPolygon | IMapPolyline;
