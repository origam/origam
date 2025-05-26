/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
import { ReactNode } from "react";
import S from "./TabView.module.scss";
import { TabViewState } from "src/components/tabView/TabViewState.ts";
import { observer } from "mobx-react-lite";
import { action } from "mobx";

export const TabView: React.FC<{
  items: ITabViewItem[];
  state: TabViewState;
  width: number
}> = observer(({ items, state, width}) => {
  return (
    <div className={S.root} style={{ width: width + "px" }}>
      <div className={S.content}>
        {items.map((x, i) => (
          <div key={x.label} className={state.activeTabIndex !== i ? S.hidden : S.visible}>
            {x.node}
          </div>
        ))}
      </div>
      <div className={S.labels}>
        {items.map((x, i) => (
          <div
            key={x.label}
            className={S.label + " " + ( state.activeTabIndex === i ? S.activeLabel : "")}
            onClick={() => action(() =>  state.activeTabIndex = i)()}>
            {x.label}
          </div>
        ))}
      </div>
    </div>
  );
});

export interface ITabViewItem {
  label: string;
  node: ReactNode
}
