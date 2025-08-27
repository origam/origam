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

import S from '@components/topLayout/TopLayout.module.scss';
import React, { ReactNode } from 'react';

export const TopLayout: React.FC<{
  topToolBar: ReactNode;
  editorArea: ReactNode;
  sideBar: ReactNode;
}> = props => {
  return (
    <div className={S.root}>
      <div className={S.topToolbar}>{props.topToolBar}</div>
      <div className={S.mainArea}>
        <div className={S.editorArea}>{props.editorArea}</div>
        <div className={S.sideBar}>{props.sideBar}</div>
      </div>
    </div>
  );
};
