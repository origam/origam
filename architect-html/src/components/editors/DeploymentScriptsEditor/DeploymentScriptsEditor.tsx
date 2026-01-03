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
import { TabView } from '@/components/tabView/TabView';
import { TabViewState } from '@/components/tabView/TabViewState';
import { RootStoreContext, T } from '@/main';
import CodeEditor from '@editors/codeEditor/CodeEditor';
import S from '@editors/DeploymentScriptsEditor/DeploymentScriptsEditor.module.scss';
import { GridEditorState } from '@editors/gridEditor/GridEditorState';
import PropertyEditor from '@editors/propertyEditor/PropertyEditor';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { useContext } from 'react';

const DeploymentScriptsEditor = ({ editorState }: { editorState: GridEditorState }) => {
  const rootStore = useContext(RootStoreContext);

  const handleInputChange = (value: any) => {
    const textProperty = editorState.properties.find(x => x.name === 'CommandText')!;
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
            label: T('SQL', 'editor_DeploymentScripts_TabLabel_Sql'),
            node: (
              <div className={S.editorBox}>
                <ActionPanel
                  title={T(
                    'SQL Editor: {0}',
                    'editor_DeploymentScripts_ActionPanelTitle_Sql',
                    editorState.properties.find(x => x.name === 'Name')?.value || '',
                  )}
                  buttons={<SaveButtonHOC />}
                />
                <CodeEditor
                  defaultLanguage="sql"
                  value={editorState.properties.find(x => x.name === 'CommandText')?.value ?? ''}
                  onChange={text => handleInputChange(text)}
                />
              </div>
            ),
          },
          {
            label: T('Settings', 'editor_DeploymentScripts_TabLabel_Settings'),
            node: (
              <div className={S.editorBox}>
                <ActionPanel
                  title={T(
                    'Settings: {0}',
                    'editor_DeploymentScripts_ActionPanelTitle_Settings',
                    editorState.properties.find(x => x.name === 'Name')?.value || '',
                  )}
                  buttons={<SaveButtonHOC />}
                />
                <div className={S.propertiesBox}>
                  <PropertyEditor
                    propertyManager={editorState}
                    properties={editorState.properties.filter(
                      x => !['CommandText', 'Inheritable', 'Ancestors'].includes(x.name),
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
};

export default DeploymentScriptsEditor;
