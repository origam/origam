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

import ActionPanel from '@/components/ActionPanel/ActionPanel';
import SaveButtonHOC from '@/components/SaveButtonHOC/SaveButtonHOC';
import { T } from '@/main';
import { DesignSurface } from '@editors/designerEditor/common/DesignSurface';
import S from '@editors/designerEditor/screenEditor/ScreenEditor.module.scss';
import { ScreenEditorState } from '@editors/designerEditor/screenEditor/ScreenEditorState';
import { ScreenToolbox } from '@editors/designerEditor/screenEditor/ScreenToolbox';
import { createContext } from 'react';

export const DesignerStateContext = createContext<ScreenEditorState | null>(null);

const ScreenEditor = ({ designerState }: { designerState: ScreenEditorState }) => {
  return (
    <DesignerStateContext.Provider value={designerState}>
      <div className={S.root}>
        <div>
          <ActionPanel
            title={T(
              'Screen editor: {0}',
              'screen_editor_title',
              designerState.screenToolbox.toolboxState.name,
            )}
            buttons={<SaveButtonHOC />}
          />
        </div>
        <div className={S.box}>
          <ScreenToolbox designerState={designerState} />
          <DesignSurface designerState={designerState} />
        </div>
      </div>
    </DesignerStateContext.Provider>
  );
};

export default ScreenEditor;
