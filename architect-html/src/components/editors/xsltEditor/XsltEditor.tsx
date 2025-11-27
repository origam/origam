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
import CodeEditor from '@editors/codeEditor/CodeEditor';
import PropertyEditor from '@editors/propertyEditor/PropertyEditor';
import S from '@editors/xsltEditor/XsltEditor.module.scss';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { useContext } from 'react';
import Button from '@components/Button/Button.tsx';
import { VscCheck, VscPlay } from 'react-icons/vsc';
import { XsltEditorState } from '@editors/gridEditor/XsltEditorState.ts';
import { showInfo } from '@/dialog/DialogUtils.tsx';
import { ParametersEditor } from '@editors/xsltEditor/ParametersEditor.tsx';
import { observer } from 'mobx-react-lite';

const XsltEditor = observer(({ editorState }: { editorState: XsltEditorState }) => {
  const rootStore = useContext(RootStoreContext);

  const getFieldName = (): 'TextStore' | 'Xsl' => {
    if (editorState.properties.find(x => x.name === 'TextStore')) {
      return 'TextStore';
    }
    return 'Xsl';
  };

  const handleTransformChange = (value: string | undefined) => {
    const textProperty = editorState.properties.find(x => x.name === getFieldName())!;
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: function* () {
        yield* editorState.onPropertyUpdated(textProperty, value);
      },
    });
  };

  const handleInputChange = (value: string | undefined) => {
    editorState.inputXml = value ?? '';
  };

  function onParametersClick() {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: function* () {
        const result = yield* editorState.getXsltParameters();
        editorState.setParameters(result.parameters);
        if (result.output) {
          rootStore.output = result.output;
        }
      },
    });
  }

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

  function handleTransform() {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: function* () {
        const result = yield* editorState.transform();
        rootStore.output = result.output;
        rootStore.sideBarTabViewState.shotOutput();
        editorState.xmlResult = result.xml ?? '';
        editorState.activeTabIndex = 3;
      },
    });
  }

  function renderActionPanel() {
    return (
      <ActionPanel
        title={
          T('Source XML', 'xsl_editor_tab2') +
          ': ' +
          (editorState.properties.find(x => x.name === 'Name')?.value || '')
        }
      >
        <Button
          type="secondary"
          title={T('Transform', 'transform_button_label')}
          prefix={<VscPlay />}
          onClick={handleTransform}
        />
        <Button
          type="secondary"
          title={T('Validate', 'validate_button_label')}
          prefix={<VscCheck />}
          onClick={handleValidate}
        />
      </ActionPanel>
    );
  }

  return (
    <div className={S.root}>
      <TabView
        width={400}
        state={editorState}
        items={[
          {
            label: T('XSL', 'xsl_editor_tab1'),
            node: (
              <div className={S.editorBox}>
                {renderActionPanel()}
                <CodeEditor
                  defaultLanguage="xml"
                  value={editorState.properties.find(x => x.name === getFieldName())?.value ?? ''}
                  onChange={text => handleTransformChange(text)}
                />
              </div>
            ),
          },
          {
            label: T('Source XML', 'xsl_editor_tab2'),
            node: (
              <div className={S.editorBox}>
                {renderActionPanel()}
                <CodeEditor
                  defaultLanguage="xml"
                  value={editorState.inputXml}
                  onChange={text => handleInputChange(text)}
                />
              </div>
            ),
          },
          {
            label: T('Input Parameters', 'xsl_editor_tab3'),
            onLabelClick: onParametersClick,
            node: (
              <div className={S.editorBox}>
                {renderActionPanel()}
                <div className={S.propertiesBox}>
                  <ParametersEditor editorState={editorState} />
                </div>
              </div>
            ),
          },
          {
            label: T('Result', 'xsl_editor_tab4'),
            node: (
              <div className={S.editorBox}>
                {renderActionPanel()}
                <CodeEditor defaultLanguage="xml" value={editorState.xmlResult} readOnly={true} />
              </div>
            ),
          },
          {
            label: T('Settings', 'xsl_editor_tab5'),
            node: (
              <div className={S.editorBox}>
                {renderActionPanel()}
                <div className={S.propertiesBox}>
                  <PropertyEditor
                    compact={true}
                    propertyManager={editorState}
                    properties={editorState.properties.filter(
                      x => x.name !== getFieldName() && x.name !== 'Package',
                    )}
                  />
                </div>
              </div>
            ),
          },
        ]}
      />
    </div>
  );
});

export default XsltEditor;
