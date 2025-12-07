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

import ActionPanelV2 from '@/components/ActionPanelV2/ActionPanelV2';
import Button from '@/components/Button/Button';
import { TabView } from '@/components/tabView/TabView';
import { TabViewState } from '@/components/tabView/TabViewState';
import { T } from '@/main';
import S from '@editors/DeploymentScriptsGeneratorEditor/DeploymentScriptsGeneratorEditor.module.scss';
import DeploymentScriptsGeneratorEditorState from '@editors/DeploymentScriptsGeneratorEditor/DeploymentScriptsGeneratorEditorState';
import { observer } from 'mobx-react-lite';

const DeploymentScriptsGeneratorEditor = observer(
  ({ editorState }: { editorState: DeploymentScriptsGeneratorEditorState }) => {
    return (
      <div className={S.root}>
        <TabView
          width={400}
          state={new TabViewState()}
          items={[
            {
              label: T('Deployment scripts', 'editor_DeploymentScriptsGenerator_TabLabel_List'),
              node: (
                <div className={S.editorBox}>
                  <ActionPanelV2
                    title={T(
                      'Deployment scripts: ({0})',
                      'editor_DeploymentScriptsGenerator_ActionPanelTitle_List',
                      editorState.filteredResults.length,
                    )}
                    buttons={
                      <>
                        <Button
                          title={
                            <>
                              Add to <strong>Model</strong>
                            </>
                          }
                          type="primary"
                          isDisabled={!editorState.isAddToModelReady()}
                          onClick={() => editorState.addToModel()}
                        />
                        <Button
                          title={
                            <>
                              Add to <strong>Deployment</strong>
                            </>
                          }
                          type="primary"
                          isDisabled={!editorState.isAddToDeploymentReady()}
                          onClick={() => editorState.addToDeployment()}
                        />
                      </>
                    }
                  />
                  <div className={S.tableWrapper}>
                    <table className={S.resultsTable}>
                      <thead>
                        <tr>
                          <th>
                            <input
                              type="checkbox"
                              checked={
                                editorState.selectedItems.size ===
                                  editorState.filteredResults.length &&
                                editorState.filteredResults.length > 0
                              }
                              onChange={e => {
                                if (e.target.checked) {
                                  editorState.selectAll();
                                } else {
                                  editorState.clearSelection();
                                }
                              }}
                            />
                          </th>
                          <th>
                            <div className={S.headerWithFilter}>
                              {T('Result', 'editor_DeploymentScriptsGenerator_Column_Result')}
                              <select
                                className={S.filterSelect}
                                value={editorState.resultFilter}
                                onChange={e => {
                                  editorState.resultFilter = e.target.value;
                                }}
                              >
                                {editorState.uniqueResultTypes.map(type => (
                                  <option key={type} value={type}>
                                    {type}
                                  </option>
                                ))}
                              </select>
                            </div>
                          </th>
                          <th>
                            {T('Platform', 'editor_DeploymentScriptsGenerator_Column_Platform')}
                          </th>
                          <th>
                            {T('Item Name', 'editor_DeploymentScriptsGenerator_Column_ItemName')}
                          </th>
                          <th>{T('Type', 'editor_DeploymentScriptsGenerator_Column_Type')}</th>
                          <th>{T('Script', 'editor_DeploymentScriptsGenerator_Column_Script')}</th>
                          <th>
                            {T('Script2', 'editor_DeploymentScriptsGenerator_Column_Script2')}
                          </th>
                        </tr>
                      </thead>
                      <tbody>
                        {editorState.filteredResults.map((x, index) => (
                          <tr key={x.schemaItemId || index}>
                            <td>
                              <input
                                type="checkbox"
                                checked={editorState.isSelected(x.schemaItemId)}
                                onChange={() => editorState.toggleSelection(x.schemaItemId!)}
                              />
                            </td>
                            <td>{x.resultType}</td>
                            <td>{x.platformName}</td>
                            <td>
                              <strong>{x.itemName}</strong>
                            </td>
                            <td>{x.schemaItemType}</td>

                            <td className={S.truncate} title={x.script}>
                              {x.script}
                            </td>
                            <td className={S.truncate} title={x.script2}>
                              {x.script2}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              ),
            },
          ]}
        />
      </div>
    );
  },
);

export default DeploymentScriptsGeneratorEditor;
