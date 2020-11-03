import { computed, flow } from "mobx";
import { parse as wktParse, stringify as wtkStringify } from "wkt";
import { getDataSourceFieldIndexByName } from "model/selectors/DataSources/getDataSourceFieldIndexByName";
import { MapRootStore } from "./MapRootStore";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import { onFieldChangeG } from "model/actions-ui/DataView/TableView/onFieldChange";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import _ from "lodash";

export class MapObjectsStore {
  constructor(private root: MapRootStore) {}

  get dataView() {
    return this.root.dataView;
  }

  get setup() {
    return this.root.mapSetupStore;
  }

  @computed get fldLocationIndex() {
    return getDataSourceFieldIndexByName(this.dataView, this.setup.mapLocationMember);
  }

  @computed get fldNameIndex() {
    return getDataSourceFieldIndexByName(this.dataView, this.setup.mapTextMember);
  }

  @computed get fldIconIndex() {
    return getDataSourceFieldIndexByName(this.dataView, this.setup.mapIconMember);
  }

  @computed get fldIconAzimuth() {
    return getDataSourceFieldIndexByName(this.dataView, this.setup.mapAzimuthMember);
  }

  @computed get fldIdentifier() {
    return getDataSourceFieldIndexByName(this.dataView, "Id");
  }

  @computed
  get mapObjects() {
    const result: IMapObject[] = [];
    if (this.setup.isReadOnlyView) {
      const tableRows = this.dataView.tableRows;

      for (let row of tableRows) {
        if (_.isArray(row)) {
          const objectGeoJson = wktParse(row[this.fldLocationIndex]);
          if (objectGeoJson)
            result.push({
              ...objectGeoJson,
              id: row[this.fldIdentifier],
              name: row[this.fldNameIndex],
              icon: row[this.fldIconIndex],
              azimuth: row[this.fldIconAzimuth],
            });
        }
      }
    } else {
      const selectedRow = getSelectedRow(this.dataView);
      if (selectedRow) {
        const row = selectedRow;
        if (_.isArray(row)) {
          const objectGeoJson = wktParse(row[this.fldLocationIndex]);
          if (objectGeoJson) {
            result.push({
              ...objectGeoJson,
              id: row[this.fldIdentifier],
              name: row[this.fldNameIndex],
              icon: row[this.fldIconIndex],
              azimuth: row[this.fldIconAzimuth],
            });
          }
        }
      }
    }
    console.log(result);
    return result;
  }

  handleGeometryChange(geoJson: any) {
    const self = this;
    return flow(function* () {
      const property = getDataViewPropertyById(self.dataView, self.setup.mapLocationMember);
      const selectedRow = getSelectedRow(self.dataView);
      console.log(property, selectedRow);
      if (property && selectedRow) {
        yield* onFieldChangeG(self.dataView)(
          undefined,
          selectedRow,
          property,
          geoJson ? wtkStringify(geoJson) : null
        );
        getDataTable(self.dataView).flushFormToTable(selectedRow);
        yield* getFormScreenLifecycle(self.dataView).onFlushData();
      }
    })();
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
