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

import { RootStoreContext } from '@/main.tsx';
import Logo from '@components/logo/Logo.tsx';
import SaveButtonHOC from '@components/SaveButtonHOC/SaveButtonHOC';
import SettingsButton from '@components/settingsButton/SettingsButton.tsx';
import { ProgressBar } from '@components/topBar/ProgressBar.tsx';
import S from '@components/topBar/TopBar.module.scss';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';

export default observer(function TopBar() {
  const rootStore = useContext(RootStoreContext);
  const activeEditor = rootStore.editorTabViewState.activeEditorState;

  return (
    <div className={S.root}>
      <ProgressBar />
      <div className={S.panel}>
        <Logo />
        <div className={S.actionBar}>
          {activeEditor ? <SaveButtonHOC /> : <div />}
          <SettingsButton />
        </div>
      </div>
    </div>
  );
});
