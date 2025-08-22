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

import { useContext } from 'react';
import { GridEditorState } from 'src/components/editors/gridEditor/GridEditorState.ts';
import { PropertyEditor } from 'src/components/editors/propertyEditor/PropertyEditor.tsx';
import { TabView } from 'src/components/tabView/TabView.tsx';
import { TabViewState } from 'src/components/tabView/TabViewState.ts';
import { runInFlowWithHandler } from 'src/errorHandling/runInFlowWithHandler.ts';
import { RootStoreContext, T } from 'src/main.tsx';
import CodeEditor from 'src/components/editors/codeEditor/CodeEditor';
import S from './XsltEditor.module.scss';

export const XsltEditor = ({ editorState }: { editorState: GridEditorState }) => {
  const rootStore = useContext(RootStoreContext);

  const getFieldName = (): 'TextStore' | 'Xsl' => {
    if (editorState.properties.find(x => x.name === 'TextStore')) {
      return 'TextStore';
    }
    return 'Xsl';
  };

  const handleInputChange = (value: any) => {
    const textProperty = editorState.properties.find(x => x.name === getFieldName())!;
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: function* () {
        yield* editorState.onPropertyUpdated(textProperty, value);
      },
    });
  };

  return (
    <div className={S.root}>
      <TabView
        width={400}
        state={new TabViewState()}
        items={[
          {
            label: T('XSL', 'xsl_editor_tab1'),
            node: (
              <CodeEditor
                defaultLanguage="xml"
                value={editorState.properties.find(x => x.name === getFieldName())?.value ?? ''}
                onChange={text => handleInputChange(text)}
              />
            ),
          },
          {
            label: T('Settings', 'xsl_editor_tab2'),
            node: (
              <PropertyEditor
                propertyManager={editorState}
                properties={editorState.properties.filter(x => x.name !== getFieldName())}
              />
            ),
          },
        ]}
      />
    </div>
  );
};
