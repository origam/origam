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

import { action, observable } from "mobx";

export interface ITouchMoveControlee {
  getInitialCoords(): { x: number; y: number };
  setCoords(coords: { x: number; y: number; dx: number; dy: number }): void;
}

export interface IObjectTouchMover {
  handlePointerDown(event: any): void;
}

export class ObjectTouchMover implements IObjectTouchMover {
  constructor(public controlee: ITouchMoveControlee) {}

  @observable isMoving = false;
  screenX0 = 0;
  screenY0 = 0;
  coordsX0 = 0;
  coordsY0 = 0;

  @action.bound
  handlePointerDown(event: any) {
    event.preventDefault();
    event.target.setCapture?.();
    const screenX = event.screenX;
    const screenY = event.screenY;
    const initCoords = this.controlee.getInitialCoords();
    this.coordsX0 = initCoords.x;
    this.coordsY0 = initCoords.y;
    this.screenX0 = screenX;
    this.screenY0 = screenY;
    this.isMoving = true;
    window.addEventListener("mousemove", this.handlePointerMove);
    window.addEventListener("mouseup", this.handlePointerUp, true);
  }

  @action.bound
  handlePointerMove(event: any) {
    event.preventDefault();
    const screenX = event.screenX;
    const screenY = event.screenY;
    const dx = screenX - this.screenX0;
    const dy = screenY - this.screenY0;
    const x = this.coordsX0 + dx;
    const y = this.coordsY0 + dy;
    this.controlee.setCoords({ x, y, dx, dy });
  }

  @action.bound
  handlePointerUp(event: any) {
    this.isMoving = false;
    window.removeEventListener("mousemove", this.handlePointerMove);
    window.removeEventListener("mouseup", this.handlePointerUp);
  }
}
