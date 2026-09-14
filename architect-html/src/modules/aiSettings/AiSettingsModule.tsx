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

import ActionPanel from '@/components/ActionPanel/ActionPanel';
import Button from '@/components/Button/Button';
import { runInFlowWithHandler } from '@/errorHandling/runInFlowWithHandler';
import { RootStoreContext, T } from '@/main';
import S from '@modules/aiSettings/AiSettingsModule.module.scss';
import AiSettingsModuleState from '@modules/aiSettings/AiSettingsModuleState';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';

const AiSettingsModule = observer(({ editorState }: { editorState: AiSettingsModuleState }) => {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);

  return (
    <div className={S.root}>
      <ActionPanel
        title={T('Origam AI settings', 'ai_settings_panel_title')}
        buttons={
          <Button
            title={T('Save', 'ai_settings_save')}
            type="primary"
            isDisabled={!editorState.isDirty || editorState.isSaving}
            onClick={() => run({ generator: () => editorState.save() })}
          />
        }
      />
      <div className={S.section}>
        <div className={S.fieldRow}>
          <div className={S.field}>
            <div className={S.label}>{T('Current router', 'ai_settings_router_label')}</div>
            <div className={S.readOnlyValue} data-test-id="ai-settings-router">
              {editorState.router}
            </div>
          </div>
          <div className={S.field}>
            <div className={S.label}>{T('Current model', 'ai_settings_model_label')}</div>
            <div className={S.readOnlyValue} data-test-id="ai-settings-model">
              {editorState.model}
            </div>
          </div>
        </div>
        {!editorState.hasApiKey && (
          <div className={S.apiKeyMissing} data-test-id="ai-settings-no-api-key">
            {T(
              'No AI API key is set, so the assistant cannot answer. Set Ai:ApiKey in the Architect server appsettings.json and restart the server.',
              'ai_settings_no_api_key',
            )}
          </div>
        )}
        <div className={S.field}>
          <div className={S.label}>{T('Prompt', 'ai_settings_prompt_label')}</div>
          <textarea
            className={S.editor}
            data-test-id="ai-settings-custom-instructions"
            spellCheck={false}
            placeholder={T(
              'Answer in Czech.\nName new entities with the Asap prefix.',
              'ai_settings_custom_placeholder',
            )}
            value={editorState.customInstructions}
            onChange={event => editorState.setCustomInstructions(event.target.value)}
          />
          <div className={S.hint}>
            {T(
              '{0} characters',
              'ai_settings_custom_length',
              editorState.customInstructions.length,
            )}
          </div>
        </div>
        <div className={S.toolSections}>
          <div className={S.toolSectionsHeader}>
            <div>
              <div className={S.label}>{T('Tool sections', 'ai_settings_tools_label')}</div>
              {editorState.swaggerUrl && (
                <div className={S.toolSectionsSource}>
                  {T(
                    'The sections and their tools are generated from the Architect API description at',
                    'ai_settings_tools_source',
                  )}{' '}
                  <a href={editorState.swaggerUrl} target="_blank" rel="noreferrer">
                    {editorState.swaggerUrl}
                  </a>
                </div>
              )}
            </div>
            <div className={S.toolSectionsActions}>
              <div className={S.toolSectionsSummary}>
                {T(
                  '{0} of {1} on',
                  'ai_settings_tools_summary',
                  editorState.enabledSectionCount,
                  editorState.sections.length,
                )}
              </div>
              <Button
                title={T('Reset to defaults', 'ai_settings_tools_reset')}
                type="secondary"
                onClick={() => editorState.resetSectionsToDefaults()}
              />
            </div>
          </div>
          {!editorState.sectionsAvailable && (
            <div className={S.toolSectionsUnavailable}>
              {T(
                'Sections unavailable. Is the Architect server running with Swagger?',
                'ai_settings_tools_unavailable',
              )}
            </div>
          )}
          <div className={S.toolSectionsList}>
            {editorState.sections.map(section => {
              const isEnabled = editorState.isSectionEnabled(section);
              const isExpanded = editorState.isSectionExpanded(section);
              return (
                <div key={section.name} className={S.sectionRow}>
                  <div className={S.toolSectionsItem}>
                    <button
                      type="button"
                      aria-expanded={isExpanded}
                      data-test-id="ai-settings-section-expand"
                      data-section={section.name}
                      className={S.toolSectionsExpander}
                      onClick={() => editorState.toggleSectionExpanded(section)}
                    >
                      <span className={isExpanded ? S.chevronOpen : S.chevron}>▶</span>
                      <span>
                        <span className={S.toolSectionsItemName}>
                          {section.name}
                          {section.tags.map(tag => (
                            <span
                              key={tag}
                              className={
                                tag === 'beta' ? S.toolSectionsTag : S.toolSectionsTagWarning
                              }
                            >
                              {tag}
                            </span>
                          ))}
                        </span>
                        <span className={S.toolSectionsItemMeta}>{section.description}</span>
                      </span>
                    </button>
                    <div className={S.toolSectionsItemCount}>
                      {T(
                        '{0} functions',
                        'ai_settings_tools_functions',
                        section.functionCount.toString(),
                      )}
                    </div>
                    <button
                      type="button"
                      role="switch"
                      aria-checked={isEnabled}
                      data-test-id="ai-settings-section-switch"
                      data-section={section.name}
                      className={isEnabled ? `${S.switch} ${S.switchOn}` : S.switch}
                      onClick={() => editorState.toggleSection(section)}
                    >
                      <span className={S.switchKnob} />
                    </button>
                  </div>
                  {isExpanded && (
                    <div className={S.functionList}>
                      {section.functions.map(sectionFunction => (
                        <div key={sectionFunction.name}>
                          <div className={S.functionName}>{sectionFunction.name}</div>
                          {sectionFunction.path && (
                            <div className={S.functionRoute}>
                              {sectionFunction.method} {sectionFunction.path}
                            </div>
                          )}
                          {sectionFunction.description && (
                            <div className={S.functionDescription}>
                              {sectionFunction.description}
                            </div>
                          )}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
});

export default AiSettingsModule;
