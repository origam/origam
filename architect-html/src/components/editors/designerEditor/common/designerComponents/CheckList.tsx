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

import { ReactElement } from 'react';
import S from '@editors/designerEditor/common/designerComponents/Components.module.scss';
import { Component } from '@editors/designerEditor/common/designerComponents/Component.tsx';
import { T } from '@/main';

export class CheckList extends Component {
  getDesignerRepresentation(): ReactElement | null {
    return (
      <div className={S.designSurfaceEditorContainer}>
        <div className={S.checkList}>
          <div className={S.checklistItem}>
            <input type="checkbox" className="checkbox undefined" readOnly />
            <div>{T('Option A', 'check_list_option_A')}</div>
          </div>
          <div className={S.checklistItem}>
            <input type="checkbox" className="checkbox undefined" readOnly />
            <div>{T('Option B', 'check_list_option_B')}</div>
          </div>
          <div className={S.checklistItem}>
            <input type="checkbox" className="checkbox undefined" readOnly />
            <div>{T('Option C', 'check_list_option_C')}</div>
          </div>
        </div>
      </div>
    );
  }
}
