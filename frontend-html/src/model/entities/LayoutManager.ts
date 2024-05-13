/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

import { Layout } from "./types/IApplication";
import { observable } from "mobx";

export class LayoutManager {

  @observable
  public layout = Layout.Desktop;

  constructor() {
    window.addEventListener("resize", this.handleResize.bind(this));
    this.handleResize();
  }

  getCurrentLayout() {
    if (window.innerHeight < 400) {
      return Layout.Phone;
    }
    if (window.innerWidth < 576) {
      return Layout.Tablet;
    }
    return Layout.Desktop;
  }

  handleResize() {
    this.layout = this.getCurrentLayout();
  }
}
