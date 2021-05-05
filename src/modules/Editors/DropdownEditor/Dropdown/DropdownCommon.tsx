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

import React from "react";
import { BoundingRect } from "react-measure";


export const CtxDropdownBodyRect = React.createContext<BoundingRect>({
  top: 0,
  left: 0,
  height: 0,
  width: 0,
  right: 0,
  bottom: 0,
});

export const CtxDropdownCtrlRect = React.createContext<BoundingRect>({
  top: 0,
  left: 0,
  height: 0,
  width: 0,
  right: 0,
  bottom: 0,
});

export const CtxDropdownRefBody = React.createContext<any>(undefined);

export const CtxDropdownRefCtrl = React.createContext<any>(undefined);