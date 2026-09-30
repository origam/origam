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

import S from '@/ai/AiPanelToggleButton.module.scss';
import OrigamAiIcon from '@/ai/OrigamAiIcon';
import { runInFlowWithHandler } from '@/errorHandling/runInFlowWithHandler';
import { RootStoreContext, T } from '@/main';
import cn from 'classnames';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { VscChevronDown } from 'react-icons/vsc';

const AiPanelToggleButton = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const isPanelVisible = rootStore.uiState.aiPanelVisible;

  return (
    <div className={S.root}>
      <div className={cn(S.button, isPanelVisible ? S.primary : S.secondary)}>
        <button
          type="button"
          className={S.main}
          data-test-id="topbar-toggle-ai"
          onClick={() => rootStore.uiState.toggleAiPanel()}
        >
          <OrigamAiIcon />
          <span>{T('AI', 'app_ai')}</span>
        </button>
        <div className={S.divider} />
        <button
          type="button"
          className={S.toggle}
          data-test-id="topbar-ai-settings"
          title={T('AI settings', 'ai_settings_tab_label')}
          aria-label={T('AI settings', 'ai_settings_tab_label')}
          onClick={() => run({ generator: rootStore.editorTabViewState.openAiSettingsModule() })}
        >
          <VscChevronDown />
        </button>
      </div>
    </div>
  );
});

export default AiPanelToggleButton;
