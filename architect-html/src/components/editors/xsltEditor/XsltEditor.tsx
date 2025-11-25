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
import { RootStoreContext, T } from '@/main';
import { TabView } from '@components/tabView/TabView';
import { TabViewState } from '@components/tabView/TabViewState';
import CodeEditor from '@editors/codeEditor/CodeEditor';
import PropertyEditor from '@editors/propertyEditor/PropertyEditor';
import S from '@editors/xsltEditor/XsltEditor.module.scss';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { useContext } from 'react';
import Button from '@components/Button/Button.tsx';
import { VscCheck, VscPlay } from 'react-icons/vsc';
import { XsltEditorState } from '@editors/gridEditor/XsltEditorState.ts';
import { showInfo } from '@/dialog/DialogUtils.tsx';

const XsltEditor = ({ editorState }: { editorState: XsltEditorState }) => {
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

  function handleValidate() {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: function* () {
        const result = yield* editorState.validate();
        rootStore.output = result.output;
        rootStore.sideBarTabViewState.shotOutput();
        yield showInfo(rootStore.dialogStack, result.title, result.text);
      },
    });
  }

  return (
    <div className={S.root}>
      <TabView
        width={400}
        state={new TabViewState()}
        items={[
          {
            label: T('XSL', 'xsl_editor_tab1'),
            node: (
              <div className={S.editorBox}>
                <ActionPanel
                  title={
                    T('XSL', 'xsl_editor_tab1') +
                    ': ' +
                    (editorState.properties.find(x => x.name === 'Name')?.value || '')
                  }
                >
                  <Button
                    type="secondary"
                    title={T('Transform', 'transform_button_label')}
                    prefix={<VscPlay />}
                    onClick={() => {}}
                  />
                  <Button
                    type="secondary"
                    title={T('Validate', 'validate_button_label')}
                    prefix={<VscCheck />}
                    onClick={handleValidate}
                  />
                </ActionPanel>
                <CodeEditor
                  defaultLanguage="xml"
                  value={editorState.properties.find(x => x.name === getFieldName())?.value ?? ''}
                  onChange={text => handleInputChange(text)}
                />
              </div>
            ),
          },
          {
            label: T('Settings', 'xsl_editor_tab2'),
            node: (
              <div className={S.editorBox}>
                <ActionPanel
                  title={
                    T('Settings', 'xsl_editor_tab2') +
                    ': ' +
                    (editorState.properties.find(x => x.name === 'Name')?.value || '')
                  }
                />
                <div className={S.propertiesBox}>
                  <PropertyEditor
                    propertyManager={editorState}
                    properties={editorState.properties.filter(x => x.name !== getFieldName())}
                  />
                </div>
              </div>
            ),
          },
        ]}
      />
    </div>
  );
};

export default XsltEditor;
