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

import { Component } from '@editors/designerEditor/common/designerComponents/Component';
import S from '@editors/designerEditor/common/designerComponents/Components.module.scss';
import { ReactElement } from 'react';

const nodeIndents = [0, 1, 2, 1, 0, 1];

export class AsTree extends Component {
  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.tree}>
        {nodeIndents.map((indent, index) => (
          <div key={index} className={S.treeNode} style={{ marginLeft: `${indent * 16}px` }} />
        ))}
      </div>
    );
  }
}
