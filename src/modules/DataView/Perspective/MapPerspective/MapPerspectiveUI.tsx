import React from "react";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import S from "./MapPerspectiveUI.module.scss";

export class MapPerspectiveCom extends React.Component {
  elmMapDiv: HTMLDivElement | null = null;
  refMapDiv = (elm: any) => (this.elmMapDiv = elm);

  componentDidMount() {
    const lmap = L.map(this.elmMapDiv!).setView([51.505, -0.09], 13);
    L.tileLayer(
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
    ).addTo(lmap);
  }

  render() {
    return <div className={S.mapDiv} ref={this.refMapDiv}></div>;
  }
}
