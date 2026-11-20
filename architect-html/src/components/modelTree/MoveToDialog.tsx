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

import { RootStoreContext, T } from '@/main';
import { IDropDownValue, IMoveTarget, IMoveTargetsResult } from '@api/IArchitectApi';
import S from '@components/modelTree/MoveToDialog.module.scss';
import { ModalWindow } from '@dialogs/ModalWindow';
import { FilterableSelect } from '@editors/propertyEditor/FilterableSelect';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import { KeyboardEvent, useContext, useMemo, useState } from 'react';

interface MoveToDialogProps {
  sourceName: string;
  initialResult: IMoveTargetsResult;
  loadTargets: (packageId: string) => Promise<IMoveTargetsResult>;
  onCancel: () => void;
  onConfirm: (target: IMoveTarget, isCopy: boolean, packageId: string | undefined) => void;
}

// Non breaking - the dropdown collapses plain spaces.
const INDENT = '  ';

export const MoveToDialog = observer(
  ({ sourceName, initialResult, loadTargets, onCancel, onConfirm }: MoveToDialogProps) => {
    const rootStore = useContext(RootStoreContext);
    const run = runInFlowWithHandler(rootStore.errorDialogController);
    const [result, setResult] = useState(initialResult);
    const [targetKey, setTargetKey] = useState(() => currentLocationKey(initialResult.targets));
    const [isLoading, setIsLoading] = useState(false);

    const targets = useMemo(
      () => [...result.targets].sort((left, right) => left.path.localeCompare(right.path)),
      [result],
    );
    const selected = targets.find(target => target.key === targetKey) ?? null;
    const activePackage = result.packages.find(movePackage => movePackage.isActive);
    const chosenPackage = result.packages.find(movePackage => movePackage.id === result.packageId);

    const targetOptions: IDropDownValue[] = targets.map(target => ({
      value: target.key,
      name:
        INDENT.repeat(target.depth) +
        (target.depth > 0 && target.packageName !== chosenPackage?.name
          ? T('{0} ({1})', 'move_to_target_in_package', target.nodeText, target.packageName)
          : target.nodeText),
    }));

    function changePackage(packageId: string) {
      run({
        generator: function* () {
          setIsLoading(true);
          try {
            const next = (yield loadTargets(packageId)) as IMoveTargetsResult;
            setResult(next);
            setTargetKey(key =>
              next.targets.some(target => target.key === key)
                ? key
                : currentLocationKey(next.targets),
            );
          } finally {
            setIsLoading(false);
          }
        },
      });
    }

    function confirmTarget(isCopy: boolean) {
      if (selected && !isLoading && (isCopy ? selected.canCopy : selected.canMove)) {
        onConfirm(selected, isCopy, result.packageId ?? undefined);
      }
    }

    function onKeyDown(event: KeyboardEvent<HTMLDivElement>) {
      if (event.key === 'Escape') {
        onCancel();
      }
    }

    return (
      <ModalWindow
        title={T('Move "{0}" to', 'move_to_title', sourceName)}
        width={460}
        buttonsRight={
          <>
            <button
              className={S.button}
              data-test-id="move-to-button-move"
              disabled={isLoading || !selected || !selected.canMove}
              onClick={() => confirmTarget(false)}
            >
              {T('Move', 'move_to_button_move')}
            </button>
            <button
              className={S.button}
              data-test-id="move-to-button-copy"
              disabled={isLoading || !selected || !selected.canCopy}
              onClick={() => confirmTarget(true)}
            >
              {T('Copy', 'move_to_button_copy')}
            </button>
            <button data-test-id="move-to-button-cancel" onClick={onCancel}>
              {T('Cancel', 'dialog_cancel')}
            </button>
          </>
        }
      >
        <div className={S.root} data-test-id="move-to-dialog" onKeyDown={onKeyDown}>
          {targets.length === 0 ? (
            <div className={S.note} data-test-id="move-to-empty">
              {T('No target available.', 'move_to_empty')}
            </div>
          ) : (
            <>
              <label className={S.row}>
                <span className={S.label}>{T('Package', 'move_to_package')}</span>
                <span className={S.field} data-test-id="move-to-package">
                  <FilterableSelect
                    options={result.packages.map(movePackage => ({
                      value: movePackage.id,
                      name: movePackage.name,
                    }))}
                    selectedValue={result.packageId}
                    autoFocus={true}
                    onChange={value => {
                      if (value && value !== result.packageId) {
                        changePackage(value);
                      }
                    }}
                  />
                </span>
              </label>
              <label className={S.row}>
                <span className={S.label}>{T('Target', 'move_to_target')}</span>
                <span className={S.field} data-test-id="move-to-target">
                  <FilterableSelect
                    options={targetOptions}
                    selectedValue={targetKey}
                    onChange={value => setTargetKey(value)}
                  />
                </span>
              </label>
              {selected?.canMove && selected.filePath && (
                <div className={S.row}>
                  <span className={S.label}>{T('File', 'move_to_file')}</span>
                  <span className={S.filePath} data-test-id="move-to-file-path">
                    {selected.filePath}
                  </span>
                </div>
              )}
              {result.packageRefusal && (
                <div className={S.note} data-test-id="move-to-package-refusal">
                  {result.packageRefusal}
                </div>
              )}
              {selected && !selected.canMove && selected.moveRefusal && (
                <div className={S.note} data-test-id="move-to-move-refusal">
                  {selected.moveRefusal}
                </div>
              )}
              {activePackage && activePackage.id !== result.packageId && (
                <div className={S.note} data-test-id="move-to-copy-note">
                  {T(
                    'A copy is always created in the active package {0}.',
                    'move_to_copy_active_package',
                    activePackage.name,
                  )}
                </div>
              )}
              {result.isTruncated && (
                <div className={S.note}>
                  {T('Too many targets, the list is incomplete.', 'move_to_truncated')}
                </div>
              )}
            </>
          )}
        </div>
      </ModalWindow>
    );
  },
);

function currentLocationKey(targets: IMoveTarget[]): string | null {
  return targets.find(target => target.isCurrentLocation)?.key ?? null;
}
