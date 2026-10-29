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

import { T } from '@/main';
import { IPackageReferenceCandidate } from '@api/IArchitectApi';
import S from '@components/packages/PackageReferencesDialog.module.scss';
import { ModalWindow } from '@dialogs/ModalWindow';
import { IDialogStackState } from '@dialogs/types';
import { action } from 'mobx';
import { observer } from 'mobx-react-lite';
import { useState } from 'react';

interface PackageReferencesDialogProps {
  packageName: string;
  candidates: IPackageReferenceCandidate[];
  onOkClick: (referencedPackageIds: string[]) => void;
  onCancelClick: () => void;
}

export const PackageReferencesDialog = observer((props: PackageReferencesDialogProps) => {
  const [selectedIds, setSelectedIds] = useState(
    () => new Set(props.candidates.filter(x => x.isReferenced).map(x => x.id)),
  );
  const isChanged = props.candidates.some(x => x.isReferenced !== selectedIds.has(x.id));

  function toggle(packageId: string) {
    const next = new Set(selectedIds);
    if (next.has(packageId)) {
      next.delete(packageId);
    } else {
      next.add(packageId);
    }
    setSelectedIds(next);
  }

  return (
    <ModalWindow
      title={T('References of {0}', 'packages_references_title', props.packageName)}
      buttonsCenter={
        <>
          <button
            className={`${S.okButton} ${isChanged ? 'isPrimary' : ''}`}
            disabled={!isChanged}
            onClick={() => props.onOkClick([...selectedIds])}
          >
            {T('OK', 'dialog_ok')}
          </button>
          <button onClick={props.onCancelClick}>{T('Cancel', 'dialog_cancel')}</button>
        </>
      }
    >
      <div className={S.content}>
        {props.candidates.map(candidate => {
          const isSelected = selectedIds.has(candidate.id);
          const isDisabled = candidate.createsCycle && !isSelected;
          return (
            <label key={candidate.id} className={S.item}>
              <input
                type="checkbox"
                checked={isSelected}
                disabled={isDisabled}
                onChange={() => toggle(candidate.id)}
                data-test-id={`package-reference-${candidate.name}`}
              />
              <span>{candidate.name}</span>
              {isDisabled && (
                <span className={S.hint}>
                  {T('would create a circular reference', 'packages_references_cycle')}
                </span>
              )}
            </label>
          );
        })}
      </div>
    </ModalWindow>
  );
});

export function askForPackageReferences(
  dialogStack: IDialogStackState,
  packageName: string,
  candidates: IPackageReferenceCandidate[],
): Promise<string[] | null> {
  return new Promise(
    action((resolve: (value: string[] | null) => void) => {
      const closeDialog = dialogStack.pushDialog(
        '',
        <PackageReferencesDialog
          packageName={packageName}
          candidates={candidates}
          onOkClick={referencedPackageIds => {
            closeDialog();
            resolve(referencedPackageIds);
          }}
          onCancelClick={() => {
            closeDialog();
            resolve(null);
          }}
        />,
      );
    }),
  );
}
