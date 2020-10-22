import React from "react";
import L from "leaflet";
import "leaflet-draw";
import "leaflet-draw/dist/leaflet.draw-src.css";
import "leaflet/dist/leaflet.css";

import S from "./MapPerspectiveUI.module.scss";

delete (L.Icon.Default.prototype as any)._getIconUrl;

L.Icon.Default.mergeOptions({
  iconRetinaUrl: require("leaflet/dist/images/marker-icon-2x.png"),
  iconUrl: require("leaflet/dist/images/marker-icon.png"),
  shadowUrl: require("leaflet/dist/images/marker-shadow.png"),
});

interface IMapPerspectiveComProps {
  mapCenter: { lat: number; lng: number };
}

export class MapPerspectiveCom extends React.Component<IMapPerspectiveComProps> {
  elmMapDiv: HTMLDivElement | null = null;
  refMapDiv = (elm: any) => (this.elmMapDiv = elm);

  leafletMap?: L.DrawMap;

  panToCenter() {
    this.leafletMap?.panTo([this.props.mapCenter.lat, this.props.mapCenter.lng]);
  }

  isPropMapCenterDifferent(prevProps: IMapPerspectiveComProps) {
    return (
      this.props.mapCenter.lat !== prevProps.mapCenter.lat ||
      this.props.mapCenter.lng !== prevProps.mapCenter.lng
    );
  }

  componentDidUpdate(prevProps: IMapPerspectiveComProps) {
    if (this.isPropMapCenterDifferent(prevProps)) {
      this.panToCenter();
    }
  }

  componentDidMount() {
    const osmUrl = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
    const osmAttrib =
      '&copy; <a href="http://openstreetmap.org/copyright">OpenStreetMap</a> contributors';
    const osm = L.tileLayer(osmUrl, { maxZoom: 18, attribution: osmAttrib });
    const lmap = L.map(this.elmMapDiv!);
    const drawnItems = L.featureGroup().addTo(lmap);
    this.leafletMap = lmap;
    lmap.setZoom(13);
    this.panToCenter();
    L.control
      .layers(
        {
          osm: osm.addTo(lmap),
          "osm-mapbox": L.tileLayer(
            "https://api.mapbox.com/styles/v1/{id}/tiles/{z}/{x}/{y}?access_token={accessToken}",
            {
              attribution:
                'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, Imagery © <a href="https://www.mapbox.com/">Mapbox</a>',
              maxZoom: 18,
              id: "mapbox/streets-v11",
              tileSize: 512,
              zoomOffset: -1,
              accessToken:
                "pk.eyJ1IjoicHRvbWFzayIsImEiOiJjazhtNHIyY20wNWF3M2VtZDE4MThia2NzIn0.g3EBH4NfxZzDBQ7DlxUeMQ",
            }
          ),
          google: L.tileLayer("http://www.google.cz/maps/vt?lyrs=s@189&gl=cn&x={x}&y={y}&z={z}", {
            attribution: "google",
          }),
        },
        { drawlayer: drawnItems },
        { position: "topleft", collapsed: false }
      )
      .addTo(lmap);
    lmap.addControl(
      new L.Control.Draw({
        edit: {
          featureGroup: drawnItems,
          poly: {
            allowIntersection: true,
          },
        },
        draw: {
          polygon: {
            allowIntersection: false,
            showArea: true,
          },
        },
      } as any) // 🦄
    );

    lmap.on(L.Draw.Event.CREATED, function (event) {
      const layer = event.layer;
      console.log(drawnItems.toGeoJSON());
      drawnItems.addLayer(layer);
    });

    lmap.on(L.Draw.Event.EDITED, function (event) {
      console.log(drawnItems.toGeoJSON());
    });

    /*const drawControl = new L.Control.Draw();
    lmap.addControl(drawControl);*/
  }

  render() {
    return <div className={S.mapDiv} ref={this.refMapDiv}></div>;
  }
}
