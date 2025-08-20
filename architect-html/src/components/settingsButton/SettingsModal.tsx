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

import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { ModalWindow } from 'src/dialog/ModalWindow.tsx';
import { RootStoreContext, T } from 'src/main';
import S from './SettingsModal.module.scss';

export const SettingsModal = observer(({ onClose }: { onClose: () => void }) => {
  const rootStore = useContext(RootStoreContext);
  const uiState = rootStore.uiState;

  return (
    <ModalWindow
      title={T('Settings', 'settings_modal_label')}
      width={600}
      height={400}
      titleButtons={
        <button tabIndex={0} autoFocus={true} onClick={onClose}>
          X
        </button>
      }
      buttonsRight={
        <button tabIndex={0} autoFocus={true} onClick={onClose}>
          {T('Close', 'settings_button_close_label')}
        </button>
      }
    >
      <div className={S.root}>
        <table className={S.table}>
          <tbody>
            <tr>
              <th>
                <label className={S.label}>
                  <input
                    type="checkbox"
                    checked={uiState.settings.isVimEnabled}
                    onChange={() => uiState.toggleVimEnabled()}
                  />
                  Vim mode
                </label>
              </th>
              <td className={S.description}>
                {T(
                  'To enable VIM-style key bindings in supported editors.',
                  'settings_modal_vim_description',
                )}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </ModalWindow>
  );
});
