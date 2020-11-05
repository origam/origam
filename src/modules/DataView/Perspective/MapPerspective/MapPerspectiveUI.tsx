import cx from "classnames";
import L from "leaflet";
import "leaflet-draw";
import "leaflet-draw/dist/leaflet.draw-src.css";
import "leaflet/dist/leaflet.css";
import _ from "lodash";
import { computed, reaction, runInAction } from "mobx";
import qs from "querystring";
import React from "react";
import S from "./MapPerspectiveUI.module.scss";
import { IMapObject, IMapObjectType } from "./stores/MapObjectsStore";
import { MapLayer } from "./stores/MapSetupStore";

delete (L.Icon.Default.prototype as any)._getIconUrl;

L.Icon.Default.mergeOptions({
  iconRetinaUrl: require("leaflet/dist/images/marker-icon-2x.png"),
  iconUrl: require("leaflet/dist/images/marker-icon.png"),
  shadowUrl: require("leaflet/dist/images/marker-shadow.png"),
});

interface IMapPerspectiveComProps {
  mapCenter: { type: "Point"; coordinates: [number, number] };
  getMapObjects: () => IMapObject[];
  lastDetailedObject?: IMapObject;
  mapLayers: MapLayer[];
  isReadOnly: boolean;
  isActive: boolean;
  onChange?(geoJson: any): void;
  onLayerClick?(id: string): void;
}

const MAP_ANIMATE_SETTING = {
  animate: true,
  duration: 1.1,
};

export class MapPerspectiveCom extends React.Component<IMapPerspectiveComProps> {
  elmMapDiv: HTMLDivElement | null = null;
  refMapDiv = (elm: any) => (this.elmMapDiv = elm);

  leafletMap?: L.DrawMap;

  leafletMapObjects = new L.FeatureGroup();

  _disposers: any[] = [];

  constructor(props: IMapPerspectiveComProps) {
    super(props);
  }

  panToCenter() {
    this.leafletMap?.panTo(
      [this.props.mapCenter.coordinates[1], this.props.mapCenter.coordinates[0]],
      { ...MAP_ANIMATE_SETTING }
    );
  }

  panToLoc(loc: [number, number]) {
    this.leafletMap?.panTo(loc, { ...MAP_ANIMATE_SETTING });
  }

  panToSelectedObject() {
    for (let [obj, lLayer] of this.mapDrawnObjectLayers) {
      if (obj.id === this.props.lastDetailedObject?.id) {
        this.panToLayer(lLayer);
        return;
      }
    }
  }

  panToLayer(lLayer: L.Layer) {
    if ((lLayer as any).getBounds) {
      const bounds = (lLayer as any).getBounds() as L.LatLngBounds;
      this.leafletMap?.fitBounds(bounds.pad(0.1), { ...MAP_ANIMATE_SETTING });
    } else if ((lLayer as any).getLatLng) {
      const latLng = (lLayer as any).getLatLng();
      this.leafletMap?.panTo(latLng, { ...MAP_ANIMATE_SETTING });
    }
  }

  panToFirstObject() {
    if (this.mapDrawnObjectLayers.length > 0) {
      this.panToLayer(this.mapDrawnObjectLayers[0][1]);
    }
  }

  isPropMapCenterDifferent(prevProps: IMapPerspectiveComProps) {
    return (
      this.props.mapCenter.coordinates[0] !== prevProps.mapCenter.coordinates[0] ||
      this.props.mapCenter.coordinates[1] !== prevProps.mapCenter.coordinates[1]
    );
  }

  componentDidUpdate(prevProps: IMapPerspectiveComProps) {
    runInAction(() => {
      if (this.isPropMapCenterDifferent(prevProps)) {
        this.panToCenter();
      }
      /*const { lastDetailedObject } = this.props;
      if (
        lastDetailedObject &&
        (!prevProps.lastDetailedObject ||
          !_.isEqual(lastDetailedObject, prevProps.lastDetailedObject))
      ) {
        this.panToSelectedObject();
        this.highlightSelectedLayer();
      }
      if (!lastDetailedObject && prevProps.lastDetailedObject) {
        this.highlightSelectedLayer();
      }*/
    });
  }

  highlightLayer(lLayer: L.Layer) {
    if ((lLayer as any).setStyle) {
      (lLayer as any).setStyle({ color: "yellow" });
    }
    if ((lLayer as any).setIcon) {
      (lLayer as any).setIcon(
        L.divIcon({
          ...(lLayer as any).getIcon().options,
          className: "markerHighlighted",
        })
      );
    }
  }

  unHighlightLayer(lLayer: L.Layer) {
    if ((lLayer as any).setStyle) {
      (lLayer as any).setStyle({ color: "blue" });
    }
    if ((lLayer as any).setIcon) {
      (lLayer as any).setIcon(
        L.divIcon({
          ...(lLayer as any).getIcon().options,
          className: "",
        })
      );
    }
  }

  highlightSelectedLayer() {
    for (let [obj, lLayer] of this.mapDrawnObjectLayers) {
      if (obj.id === this.props.lastDetailedObject?.id) {
        this.highlightLayer(lLayer);
      } else {
        this.unHighlightLayer(lLayer);
      }
    }
  }

  @computed get layerList() {
    return this.props.mapLayers
      .map((rawLayer, index) => {
        if (rawLayer.type === "OSM") {
          return [
            rawLayer,
            L.tileLayer(rawLayer.getUrl(), { ...rawLayer.getOptions(), zIndex: index }),
          ];
        } else if (rawLayer.type === "WMS") {
          return [
            rawLayer,
            L.tileLayer.wms(rawLayer.getUrl(), { ...rawLayer.getOptions(), zIndex: index }),
          ];
        }
      })
      .filter((layer) => layer) as [MapLayer, L.TileLayer][];
  }

  @computed get leafletlayersDescriptor() {
    console.log(this.layerList);
    return Object.fromEntries(
      this.layerList.map((layer) => {
        return [layer[0].getTitle(), layer[1]];
      })
    );
  }

  @computed get mapDrawnObjectLayers() {
    return this.props
      .getMapObjects()
      .map((obj) => {
        let result: [IMapObject, L.Layer];
        switch (obj.type) {
          case IMapObjectType.POINT:
            {
              const iconUrl = obj.icon;
              const pq = qs.parse(iconUrl.split("#")[1] || "");
              const anchor = pq.anchor ? JSON.parse(pq.anchor as string) : [0, 0];
              const iconAnchor: [number, number] = anchor;
              const iconRotation = obj.azimuth || 0;
              const myIcon = L.divIcon({
                html: `<img src="${iconUrl}" style="
                transform: rotate3d(0,0,1,${iconRotation}deg);
                transform-origin: ${iconAnchor[0]}px ${iconAnchor[1]}px;" />`,
                // iconSize: [38, 95],
                iconAnchor,
                className: "",
                //popupAnchor: [-3, -76],
              });
              result = [
                obj,
                L.marker([obj.coordinates[1], obj.coordinates[0]], {
                  icon: myIcon,
                }).bindTooltip(obj.name),
              ];
            }
            break;
          case IMapObjectType.POLYGON:
            {
              result = [
                obj,
                L.polygon(
                  obj.coordinates[0].map((coords) => [coords[1], coords[0]]),
                  { color: "blue" }
                ).bindTooltip(obj.name),
              ];
            }
            break;
          case IMapObjectType.LINESTRING:
            {
              result = [
                obj,
                L.polyline(
                  obj.coordinates.map((coords) => [coords[1], coords[0]]),
                  { color: "blue" }
                ).bindTooltip(obj.name),
              ];
            }
            break;
        }
        return result;
      })
      .filter((layer) => layer) as [IMapObject, L.Layer][];
  }

  initLeafletDrawControls() {
    this.leafletMap?.addControl(
      new L.Control.Draw({
        edit: {
          featureGroup: this.leafletMapObjects,
          poly: {
            allowIntersection: true,
          },
          circle: false,
          circlemarker: false,
        },
        draw: {
          polygon: {
            allowIntersection: false,
            showArea: true,
          },
          circle: false,
          circlemarker: false,
        },
      } as any) // 🦄
    );

    this.leafletMap?.on(L.Draw.Event.CREATED, (event) => {
      const layer = event.layer;
      this.leafletMapObjects.clearLayers();
      this.leafletMapObjects.addLayer(layer);
      console.log(layer.toGeoJSON());
      const obj = layer.toGeoJSON().geometry;
      this.props.onChange?.(obj);
    });

    this.leafletMap?.on(L.Draw.Event.EDITED, (event) => {
      const layers = (event as any).layers;
      console.log(layers.toGeoJSON());
      const obj = layers.toGeoJSON().features?.[0]?.geometry;
      console.log(obj);
      if (obj) {
        this.props.onChange?.(obj);
      }
    });

    this.leafletMap?.on(L.Draw.Event.DELETED, (event) => {
      this.props.onChange?.(undefined);
    });
  }

  initLeaflet() {
    const lmap = L.map(this.elmMapDiv!, {
      layers: this.layerList
        .filter(([rawLayer, leaLayer]) => rawLayer.defaultEnabled)
        .map(([rawLayer, leaLayer]) => leaLayer),
    });
    this.leafletMap = lmap;
    lmap.setZoom(5);
    this.panToCenter();
    L.control
      .layers({}, this.leafletlayersDescriptor, { position: "topleft", collapsed: true })
      .addTo(lmap);
    L.control.scale().addTo(lmap);

    lmap.addLayer(this.leafletMapObjects);

    this._disposers.push(
      reaction(
        () => this.mapDrawnObjectLayers,
        (layers) => {
          console.log("Drawing layers", layers);
          this.leafletMapObjects.clearLayers();
          for (let layer of layers) {
            this.leafletMapObjects.addLayer(layer[1]);
            layer[1].on("click", () => this.props.onLayerClick?.(layer[0].id));
          }
          this.highlightSelectedLayer();
        },
        {
          delay: 100,
          fireImmediately: true,
        }
      )
    );
  }

  componentDidMount() {
    this.initLeaflet();
    if (!this.props.isReadOnly) {
      this.initLeafletDrawControls();
    }
  }

  componentWillUnmount() {
    this._disposers.forEach((d) => d());
  }

  render() {
    return (
      <div className={cx(S.mapDiv, { isHidden: !this.props.isActive })} ref={this.refMapDiv}></div>
    );
  }
}
