import cx from "classnames";
import L from "leaflet";
import "leaflet-draw";
import "leaflet-draw/dist/leaflet.draw-src.css";
import "leaflet/dist/leaflet.css";
import { computed, reaction } from "mobx";
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
  mapLayers: MapLayer[];
  isReadOnly: boolean;
  isActive: boolean;
  onChange?(geoJson: any): void;
}

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
    this.leafletMap?.panTo([
      this.props.mapCenter.coordinates[1],
      this.props.mapCenter.coordinates[0],
    ]);
  }

  isPropMapCenterDifferent(prevProps: IMapPerspectiveComProps) {
    return (
      this.props.mapCenter.coordinates[0] !== prevProps.mapCenter.coordinates[0] ||
      this.props.mapCenter.coordinates[1] !== prevProps.mapCenter.coordinates[1]
    );
  }

  componentDidUpdate(prevProps: IMapPerspectiveComProps) {
    if (this.isPropMapCenterDifferent(prevProps)) {
      this.panToCenter();
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

  @computed get layerObject() {
    console.log(this.layerList);
    return Object.fromEntries(
      this.layerList.map((layer) => {
        return [layer[0].getTitle(), layer[1]];
      })
    );
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
    L.control.layers({}, this.layerObject, { position: "topleft", collapsed: true }).addTo(lmap);

    lmap.addLayer(this.leafletMapObjects);

    this._disposers.push(
      reaction(
        () => this.props.getMapObjects(),
        (objects) => {
          console.log("Drawing layers", objects);
          this.leafletMapObjects.clearLayers();
          for (let obj of objects) {
            console.log("Drawing object of layer", obj);
            switch (obj.type) {
              case IMapObjectType.POINT:
                {
                  //const iconUrl = "img/map/marker-icon.png#anchor=[12,41]";
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
                  const point = L.marker([obj.coordinates[1], obj.coordinates[0]], { icon: myIcon })
                    .bindTooltip(obj.name)
                    .addTo(this.leafletMapObjects);
                }
                break;
              case IMapObjectType.POLYGON:
                console.log(
                  "Drawing polygon",
                  obj.coordinates.length,
                  obj.coordinates.map((coords) => coords.map((coord) => [coord[1], coord[0]]))
                );
                {
                  const polygon = L.polygon(
                    obj.coordinates[0].map((coords) => [coords[1], coords[0]]),
                    { color: "blue" }
                  )
                    .bindTooltip(obj.name)
                    .addTo(this.leafletMapObjects);
                }
                break;
              case IMapObjectType.LINESTRING:
                {
                  const polygon = L.polyline(
                    obj.coordinates.map((coords) => [coords[1], coords[0]]),
                    { color: "blue" }
                  )
                    .bindTooltip(obj.name)
                    .addTo(this.leafletMapObjects);
                }
                break;
            }
          }
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

