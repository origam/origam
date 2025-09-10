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

import { RootStoreContext } from '@/main';
import S from '@components/topBar/ProgressBar.module.scss';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { createPortal } from 'react-dom';

const Overlay = observer(() => {
  const progressBarState = useContext(RootStoreContext).progressBarState;

  return createPortal(
    <>{progressBarState.isWorking && <div className={S.overlay} />}</>,
    document.getElementById('modal-window-portal')!,
  );
});

const ProgressBar = observer(() => {
  const progressBarState = useContext(RootStoreContext).progressBarState;

  return (
    <>
      {(progressBarState.isWorking ||
        window.localStorage.getItem('debugKeepProgressIndicatorsOn')) && (
        <div className={S.progressIndicator}>
          <div className={S.indefinite} />
        </div>
      )}
      <Overlay />
    </>
  );
});

export default ProgressBar;
