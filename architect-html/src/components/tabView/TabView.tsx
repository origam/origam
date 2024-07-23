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

import React, { ReactNode, useEffect, useState } from "react";
import S from "./TabView.module.scss";

export const TabView: React.FC<{
  items: TabViewItem[];
}> = (props) => {
  const [activeTab, setActiveTab] = useState<TabViewId | undefined>()

  useEffect(() => {
    setActiveTab(props.items[0].id)
  }, [])

  return (
    <div className={S.root}>
      <div className={S.content}>
        {props.items.map(x =>
          <div key={x.id} className={activeTab !== x.id ? S.hidden : S.visible}>{x.node}</div>
        )}
      </div>
      <div className={S.labels}>
        {props.items.map(x =>
          <div key={x.id} onClick={() => setActiveTab(x.id)}>
            {x.label}
          </div>
        )}
      </div>
    </div>
  );
}

export interface TabViewItem {
  id: TabViewId;
  label: string;
  node: ReactNode
}

export enum TabViewId { Packages, Model}
