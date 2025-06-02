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

import React from 'react';
import S from 'src/components/editors/gridEditor/GridEditor.module.scss';
import { PropertyEditor } from 'src/components/editors/propertyEditor/PropertyEditor.tsx';
import { GridEditorState } from 'src/components/editors/gridEditor/GridEditorState.ts';
import { observer } from 'mobx-react-lite';

export const GridEditor: React.FC<{
  editorState: GridEditorState
  title: string
}> = observer( (props) => {
  return (
    <div className={S.gridEditor}>
      <h3 className={S.title}>{props.title}</h3>
      <PropertyEditor
        propertyManager={props.editorState}
        properties={props.editorState.properties}
      />
    </div>
  );
});
