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
import S from "gui/Components/WorkbenchPage/WorkbenchPage.module.scss";
import SSplitter from "gui/Workbench/ScreenArea/CustomSplitter.module.scss";
import { Splitter } from "gui/Components/Splitter/Splitter";
import { Breakpoint } from "react-socks";

export const WorkbenchPage: React.FC<{
  sidebar: React.ReactNode;
  mainbar: React.ReactNode;
}> = props => (
  <>
    <Breakpoint small down className={S.mobileContainer}>
      {props.mainbar}
    </Breakpoint>
    <Breakpoint small up className={S.root}>
      <Splitter
        type="isHoriz"
        STYLE={SSplitter}
        panels={[
          ["sidebar", 1, props.sidebar],
          ["mainbar", 5, props.mainbar]
        ]}
      />
    </Breakpoint>
  </>
);
