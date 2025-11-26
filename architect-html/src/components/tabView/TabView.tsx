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

import S from '@components/tabView/TabView.module.scss';
import { TabViewState } from '@components/tabView/TabViewState';
import { action } from 'mobx';
import { observer } from 'mobx-react-lite';
import { ReactNode } from 'react';

export const TabView: React.FC<{
  items: ITabViewItem[];
  state: TabViewState;
  width: number;
}> = observer(({ items, state, width }) => {
  const onLabelClick = action((item: ITabViewItem, index: number) => {
    state.activeTabIndex = index;
    item.onLabelClick?.();
  });

  return (
    <div className={S.root} style={{ width: width + 'px' }}>
      <div className={S.content}>
        {items.map((item, i) => (
          <div key={item.label} className={state.activeTabIndex !== i ? S.hidden : S.visible}>
            {item.node}
          </div>
        ))}
      </div>
      <div className={S.labels}>
        {items.map((item, i) => (
          <div
            key={item.label}
            className={S.label + ' ' + (state.activeTabIndex === i ? S.activeLabel : '')}
            onClick={() => onLabelClick(item, i)}
          >
            {item.label}
          </div>
        ))}
      </div>
    </div>
  );
});

export interface ITabViewItem {
  label: string;
  node: ReactNode;
  onLabelClick?: () => void;
}
