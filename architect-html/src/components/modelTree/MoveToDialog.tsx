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
import { IDropDownValue, IMoveTarget } from '@api/IArchitectApi';
import S from '@components/modelTree/MoveToDialog.module.scss';
import { ModalWindow } from '@dialogs/ModalWindow';
import { FilterableSelect } from '@editors/propertyEditor/FilterableSelect';
import { observer } from 'mobx-react-lite';
import { KeyboardEvent, useMemo, useState } from 'react';

interface MoveToDialogProps {
  sourceName: string;
  targets: IMoveTarget[];
  isSourceInActivePackage: boolean;
  isTruncated: boolean;
  onCancel: () => void;
  onConfirm: (target: IMoveTarget, isCopy: boolean) => void;
}

// Two non breaking spaces - the dropdown collapses plain ones, so the indent would vanish.
const INDENT = '  ';

export const MoveToDialog = observer(
  ({
    sourceName,
    targets,
    isSourceInActivePackage,
    isTruncated,
    onCancel,
    onConfirm,
  }: MoveToDialogProps) => {
    const packages = useMemo(
      () => [...new Set(targets.map(target => target.packageName))].sort(),
      [targets],
    );
    const [packageName, setPackageName] = useState(() => defaultPackage(targets, packages));
    const [targetKey, setTargetKey] = useState<string | null>(null);

    const packageTargets = useMemo(
      () =>
        targets
          .filter(target => target.packageName === packageName)
          .sort((left, right) => left.path.localeCompare(right.path)),
      [targets, packageName],
    );
    const selected = packageTargets.find(target => target.key === targetKey) ?? null;

    // The path starts with the provider, so its depth is the nesting level.
    const targetOptions: IDropDownValue[] = packageTargets.map(target => ({
      value: target.key,
      name: INDENT.repeat(target.path.split('/').length - 1) + target.nodeText,
    }));

    function confirmTarget(isCopy: boolean) {
      if (selected && (isCopy ? selected.canCopy : selected.canMove)) {
        onConfirm(selected, isCopy);
      }
    }

    // A select that closed its own dropdown has already stopped the event.
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
              disabled={!selected || !selected.canMove}
              onClick={() => confirmTarget(false)}
            >
              {T('Move', 'move_to_button_move')}
            </button>
            <button
              className={S.button}
              data-test-id="move-to-button-copy"
              disabled={!selected || !selected.canCopy}
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
                    options={packages.map(name => ({ value: name, name: name }))}
                    selectedValue={packageName}
                    autoFocus={true}
                    onChange={value => {
                      setPackageName(value);
                      setTargetKey(null);
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
              {selected && !selected.canMove && !isSourceInActivePackage && (
                <div className={S.note} data-test-id="move-to-copy-only">
                  {T(
                    'Only a copy can be created, {0} is not in the active package.',
                    'move_to_copy_only',
                    sourceName,
                  )}
                </div>
              )}
              {selected && !selected.canMove && isSourceInActivePackage && (
                <div className={S.note} data-test-id="move-to-copy-only-target">
                  {T('Only a copy can be created for this target.', 'move_to_copy_only_target')}
                </div>
              )}
              {isTruncated && (
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

function defaultPackage(targets: IMoveTarget[], packages: string[]): string {
  const current = targets.find(target => target.isCurrentLocation);
  const active = targets.find(target => target.isInActivePackage);
  return current?.packageName ?? active?.packageName ?? packages[0] ?? '';
}
