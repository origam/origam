/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

import S from '@editors/designerEditor/common/designerComponents/Components.module.scss';
import { ReactNode } from 'react';

const Icon = (props: { name: string; isActive?: boolean; children: ReactNode }) => (
  <svg
    data-test-id={`navigation-panel-${props.name}`}
    className={props.isActive ? S.activeView : undefined}
    viewBox="0 0 20 20"
  >
    {props.children}
  </svg>
);

export const NavigationPanel = (props: {
  title: string;
  showNewButton: boolean;
  showDeleteButton: boolean;
  showCopyButton: boolean;
  showRowNavigation: boolean;
}) => (
  <div className={S.navigationPanel} data-test-id="navigation-panel">
    <div className={S.navigationPanelTitle}>{props.title}</div>
    {(props.showNewButton || props.showDeleteButton) && (
      <div className={S.navigationPanelGroup}>
        {props.showNewButton && (
          <Icon name="add">
            <circle cx="10" cy="10" r="10" />
            <line x1="10" y1="5" x2="10" y2="15" />
            <line x1="5" y1="10" x2="15" y2="10" />
          </Icon>
        )}
        {props.showDeleteButton && (
          <Icon name="delete">
            <circle cx="10" cy="10" r="10" />
            <line x1="5" y1="10" x2="15" y2="10" />
          </Icon>
        )}
        {props.showCopyButton && (
          <Icon name="copy">
            <polygon points="8.8 3 1 3 1 20 14.1 20 14.1 8.2 8.8 3" />
            <polyline points="14.1 8.2 8.8 8.2 8.8 3" />
            <polyline points="14.1 17 19 17 19 5.2 13.8 0 5.9 0 5.9 3" />
            <polyline points="19 5.2 13.8 5.2 13.8 0" />
          </Icon>
        )}
      </div>
    )}
    <div className={S.navigationPanelSpacer} />
    {props.showRowNavigation && (
      <div className={S.navigationPanelGroup}>
        <Icon name="first">
          <polygon points="17.6 18 17.6 2 5.1 10" />
          <line x1="2.4" y1="18" x2="2.4" y2="2" />
        </Icon>
        <Icon name="previous">
          <polygon points="15.3 18 15.3 2 2.7 10" />
        </Icon>
        <Icon name="next">
          <polygon points="4.7 2 4.7 18 17.3 10" />
        </Icon>
        <Icon name="last">
          <polygon points="2.4 2 2.4 18 14.9 10" />
          <line x1="17.6" y1="2" x2="17.6" y2="18" />
        </Icon>
      </div>
    )}
    <div className={S.navigationPanelGroup}>
      <Icon name="form" isActive={true}>
        <rect width="8" height="8" />
        <rect x="12" width="8" height="8" />
        <rect y="12" width="8" height="8" />
        <rect x="12" y="12" width="8" height="8" />
      </Icon>
      <Icon name="grid">
        <rect width="20" height="20" />
        <line x1="0" y1="4" x2="20" y2="4" />
        <line x1="0" y1="8" x2="20" y2="8" />
        <line x1="0" y1="12" x2="20" y2="12" />
        <line x1="0" y1="16" x2="20" y2="16" />
        <line x1="7" y1="0" x2="7" y2="20" />
      </Icon>
    </div>
    <div className={S.navigationPanelGroup}>
      <Icon name="filter">
        <rect y="1.7" width="20" height="3.33" />
        <polyline points="17.5 5 11.7 13.3 11.7 17.5 8.3 20 8.3 13.3 2.5 5" />
      </Icon>
    </div>
    <div className={S.navigationPanelGroup}>
      <Icon name="menu">
        <circle cx="2" cy="10" r="2" />
        <circle cx="10" cy="10" r="2" />
        <circle cx="18" cy="10" r="2" />
      </Icon>
    </div>
  </div>
);
