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
import React, { ReactNode, useEffect } from "react";
import { useSelector, useDispatch } from 'react-redux';
import { RootState } from 'src/stores/store.ts';
import {
  selectTabState,
  setActiveTab
} from 'src/components/tabView/TabViewSlice.ts';
import S from "./TabView.module.scss";

export const TabView: React.FC<{
  items: TabViewItem[];
  instanceId: string;
  defaultActiveTab?: TabViewId;
}> = ({ items, instanceId, defaultActiveTab }) => {
  const dispatch = useDispatch();
  const activeTab = useSelector((state: RootState) => selectTabState(state, instanceId));

  useEffect(() => {
    if (items.length > 0 && activeTab === undefined) {
      dispatch(setActiveTab({ instanceId, tabId: defaultActiveTab || items[0].id }));
    }
  }, [dispatch, items, activeTab, instanceId, defaultActiveTab]);

  return (
    <div className={S.root}>
      <div className={S.content}>
        {items.map(x => (
          <div key={x.id} className={activeTab !== x.id ? S.hidden : S.visible}>
            {x.node}
          </div>
        ))}
      </div>
      <div className={S.labels}>
        {items.map(x => (
          <div key={x.id} onClick={() => dispatch(setActiveTab({ instanceId, tabId: x.id }))}>
            {x.label}
          </div>
        ))}
      </div>
    </div>
  );
}
export interface TabViewItem {
  id: TabViewId;
  label: string;
  node: ReactNode
}

export enum TabViewId { Packages, Model }