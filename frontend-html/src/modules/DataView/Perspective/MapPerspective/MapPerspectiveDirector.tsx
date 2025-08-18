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

import React, { PropsWithChildren, useContext, useEffect } from "react";
import cx from "classnames";
import { action, flow } from "mobx";
import { IDataViewBodyUI, IDataViewToolbarUI } from "modules/DataView/DataViewUI";
import { TypeSymbol } from "dic/Container";
import { SectionViewSwitchers } from "modules/DataView/DataViewTypes";
import { getIdent, IIId } from "utils/common";
import { DataViewHeaderAction } from "gui/Components/DataViewHeader/DataViewHeaderAction";
import { Icon } from "gui/Components/Icon/Icon";
import { IMapPerspective, MapPerspective } from "./MapPerspective";
import { Observer } from "mobx-react";
import { IPerspective } from "../Perspective";
import { MapPerspectiveCom } from "./MapPerspectiveUI";
import { CtxDataViewHeaderExtension, IDataViewHeaderExtensionItem, } from "gui/Components/ScreenElements/DataView";
import { MapPerspectiveSearch } from "./MapPerspectiveSearch";
import { CtxMapRootStore, MapRootStore } from "./stores/MapRootStore";
import S from "./MapPerspectiveUI.module.scss";

export class MapPerspectiveDirector implements IIId {
  $iid = getIdent();

  constructor(
    public dataViewToolbarUI = IDataViewToolbarUI(),
    public dataViewBodyUI = IDataViewBodyUI(),
    public mapPerspective = IMapPerspective(),
    public perspective = IPerspective()
  ) {
    this.toolbarActionsExtension = new ToolbarActionsExtension(this.mapPerspective, () => this.rootStore);
  }

  rootStore: MapRootStore = null!;

  toolbarActionsExtension: ToolbarActionsExtension;

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
              onMouseDown={flow(this.mapPerspective.handleToolbarBtnClick)}
              isActive={this.mapPerspective.isActive}
            >
              <Icon src="./icons/geo-coordinates.svg"/>
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
  }, []); // eslint-disable-line react-hooks/exhaustive-deps
  return <>{props.children}</>;
}

class ToolbarActionsExtension implements IDataViewHeaderExtensionItem {
  constructor(public mapPerspective: MapPerspective, public getRootStore: () => MapRootStore) {
  }

  get rootStore() {
    return this.getRootStore();
  }

  $iid = getIdent();
  group = "actions";

  render(): React.ReactNode {
    return this.mapPerspective.isActive ? (
      <CtxMapRootStore.Provider value={this.rootStore} key={this.$iid}>
        <MapPerspectiveNavigation/>
        <MapPerspectiveSearch/>
        {/*<MapPerspectiveRoutefind />*/}
      </CtxMapRootStore.Provider>
    ) : null;
  }
}

function MapContentUI(props: {
  toolbarActionsExtension: ToolbarActionsExtension;
  mapPerspective: MapPerspective;
}) {
  const {
    mapSetupStore,
    mapObjectsStore,
    mapSearchStore,
    mapNavigationStore,
    mapRoutefinderStore,
  } = useContext(CtxMapRootStore);
  return (
    <Observer>
      {() => (
        <MapPerspectiveComContainer toolbarActionsExtension={props.toolbarActionsExtension}>
          <MapPerspectiveCom
            ref={mapNavigationStore.refMapComponent}
            lastDetailedObject={mapSearchStore.selectedSearchResult}
            mapCenter={mapSetupStore.mapCenter || undefined}
            initialZoom={mapSetupStore.mapZoom}
            getMapObjects={() => (mapRoutefinderStore.isActive ? [] : mapObjectsStore.mapObjects)}
            getRoutefinderRoute={() => mapRoutefinderStore.mapObjectsRoute}
            getRoutefinderEditables={() => mapRoutefinderStore.mapObjectsEditable}
            mapLayers={mapSetupStore.layers}
            isReadOnly={mapSetupStore.isReadOnlyView}
            isActive={props.mapPerspective.isActive}
            onChange={(geoJson) => {
              mapObjectsStore.handleGeometryChange(geoJson);
            }}
            onRoutefinderGeometryChange={mapRoutefinderStore.handleGeometryChange}
            onRoutefinderGeometryEditStart={mapRoutefinderStore.handleEditingStarted}
            onRoutefinderGeometryEditSave={mapRoutefinderStore.handleEditingFinished}
            onRoutefinderGeometryEditCancel={mapRoutefinderStore.handleEditingCancelled}
            onLayerClick={mapObjectsStore.handleLayerClick}
          />
        </MapPerspectiveComContainer>
      )}
    </Observer>
  );
}

function MapPerspectiveNavigation() {
  const {mapObjectsStore, mapNavigationStore} = useContext(CtxMapRootStore);
  useEffect(() => mapObjectsStore.handleMapMounted(), []); // eslint-disable-line react-hooks/exhaustive-deps
  return (
    <Observer>
      {() => (
        <>
          <button
            className={cx(S.mapToolbarButton)}
            onClick={mapNavigationStore.handleCenterMapClick}
          >
            <i className="fas fa-crosshairs fa-lg"/>
          </button>
          <button
            className={cx(S.mapToolbarButton)}
            onClick={mapNavigationStore.handleLookupObjectClick}
          >
            <i className="fas fa-search-location fa-lg"/>
          </button>
        </>
      )}
    </Observer>
  );
}