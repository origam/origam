import React, { PropsWithChildren, useContext, useEffect } from "react";
import { action, flow } from "mobx";
import { IDataViewBodyUI, IDataViewToolbarUI } from "modules/DataView/DataViewUI";
import { TypeSymbol } from "dic/Container";
import { SectionViewSwitchers } from "modules/DataView/DataViewTypes";
import { getIdent, IIId } from "utils/common";
import { DataViewHeaderAction } from "gui02/components/DataViewHeader/DataViewHeaderAction";
import { Icon } from "gui02/components/Icon/Icon";
import { IMapPerspective, MapPerspective } from "./MapPerspective";
import { Observer } from "mobx-react";
import { IPerspective } from "../Perspective";
import { MapPerspectiveCom } from "./MapPerspectiveUI";
import {
  CtxDataViewHeaderExtension,
  IDataViewHeaderExtensionItem,
} from "gui/Components/ScreenElements/DataView";
import { MapPerspectiveSearch } from "./MapPerspectiveSearch";
import { CtxMapRootStore, MapRootStore } from "./stores/MapRootStore";

export class MapPerspectiveDirector implements IIId {
  $iid = getIdent();

  constructor(
    public dataViewToolbarUI = IDataViewToolbarUI(),
    public dataViewBodyUI = IDataViewBodyUI(),
    public mapPerspective = IMapPerspective(),
    public perspective = IPerspective()
  ) {}

  rootStore: MapRootStore = null!;

  toolbarActionsExtension = new ToolbarActionsExtension(this.mapPerspective, () => this.rootStore);

  @action.bound
  setup() {
    this.dataViewBodyUI.contrib.put({
      $iid: this.$iid,
      render: () => (
        <CtxMapRootStore.Provider value={this.rootStore} key={this.$iid}>
          <MapContentUI
            mapPerspective={this.mapPerspective}
            toolbarActionsExtension={this.toolbarActionsExtension}
          />
        </CtxMapRootStore.Provider>
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

export function MapPerspectiveComContainer(
  props: PropsWithChildren<{ toolbarActionsExtension: ToolbarActionsExtension }>
) {
  const toolbarExtension = useContext(CtxDataViewHeaderExtension);
  useEffect(() => {
    toolbarExtension.put(props.toolbarActionsExtension);
    return () => toolbarExtension.del(props.toolbarActionsExtension);
  }, []);
  return <>{props.children}</>;
}

class ToolbarActionsExtension implements IDataViewHeaderExtensionItem {
  constructor(public mapPerspective: MapPerspective, public getRootStore: () => MapRootStore) {}

  get rootStore() {
    return this.getRootStore();
  }

  $iid = getIdent();
  group = "actions";

  render(): React.ReactNode {
    return this.mapPerspective.isActive ? (
      <CtxMapRootStore.Provider value={this.rootStore} key={this.$iid}>
        <MapPerspectiveNavigation />
        <MapPerspectiveSearch />
      </CtxMapRootStore.Provider>
    ) : null;
  }
}

function MapContentUI(props: {
  toolbarActionsExtension: ToolbarActionsExtension;
  mapPerspective: MapPerspective;
}) {
  const { mapSetupStore, mapObjectsStore, mapSearchStore, mapNavigationStore } = useContext(
    CtxMapRootStore
  );
  return (
    <Observer>
      {() => (
        <MapPerspectiveComContainer toolbarActionsExtension={props.toolbarActionsExtension}>
          <MapPerspectiveCom
            ref={mapNavigationStore.refMapComponent}
            lastDetailedObject={mapSearchStore.selectedSearchResult}
            mapCenter={mapSetupStore.mapCenter || { type: "Point", coordinates: [0, 0] }}
            getMapObjects={() => mapObjectsStore.mapObjects}
            mapLayers={mapSetupStore.layers}
            isReadOnly={mapSetupStore.isReadOnlyView}
            isActive={props.mapPerspective.isActive}
            onChange={(geoJson) => {
              console.log("Change: ", geoJson);
              mapObjectsStore.handleGeometryChange(geoJson);
            }}
            onLayerClick={mapObjectsStore.handleLayerClick}
          />
        </MapPerspectiveComContainer>
      )}
    </Observer>
  );
}

function MapPerspectiveNavigation() {
  const { mapObjectsStore, mapNavigationStore } = useContext(CtxMapRootStore);
  useEffect(() => mapObjectsStore.handleMapActivated(), []);
  return (
    <Observer>
      {() => (
        <>
          <button onClick={mapNavigationStore.handleCenterMapClick}>
            <i className="fas fa-crosshairs" />
          </button>
          <button onClick={mapNavigationStore.handleLookupObjectClick}>
            <i className="fas fa-search-location"></i>
          </button>
        </>
      )}
    </Observer>
  );
}
