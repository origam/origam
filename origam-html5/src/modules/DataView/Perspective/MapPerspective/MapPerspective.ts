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

import {TypeSymbol} from "dic/Container";
import {observable} from "mobx";
import {getIdent, IIId} from "utils/common";
import {IPerspective, IPerspectiveContrib} from "../Perspective";
import bind from "bind-decorator";
import {IPanelViewType} from "model/entities/types/IPanelViewType";
import {IViewConfiguration} from "modules/DataView/ViewConfiguration";

export class MapPerspective implements IIId, IPerspectiveContrib {
  $iid = getIdent();

  constructor(
    public perspective = IPerspective(),
    public viewConfiguration = IViewConfiguration()
  ) {}

  @observable isActive = false;

  @bind
  *handleToolbarBtnClick() {
    if (this.isActive) return;
    yield* this.perspective.deactivate();
    this.isActive = true;
    yield* this.viewConfiguration.anounceActivePerspective(IPanelViewType.Map);
  }

  @bind
  *deactivate() {
    this.isActive = false;
  }

  @bind
  *activateDefault() {
    if (this.viewConfiguration.activePerspective === IPanelViewType.Map) this.isActive = true;
  }
}

export const IMapPerspective = TypeSymbol<MapPerspective>("IMapPerspective");
