import { computed, observable } from "mobx";
import { IDataView } from "model/entities/types/IDataView";
import { getDataSourceFieldIndexByName } from "model/selectors/DataSources/getDataSourceFieldIndexByName";
import { createContext } from "react";
import { MapNavigationStore } from "./MapNavigationStore";
import { MapObjectsStore } from "./MapObjectsStore";
import { SearchStore } from "./MapSearchStore";
import { MapSetupStore } from "./MapSetupStore";

export class MapRootStore {
  constructor(public dataView: IDataView) {}

  mapSearchStore = new SearchStore(this);
  mapObjectsStore = new MapObjectsStore(this);
  mapSetupStore = new MapSetupStore(this);
  mapNavigationStore = new MapNavigationStore(this);
}

export const CtxMapRootStore = createContext<MapRootStore>(null!);



