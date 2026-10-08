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

import { RootStoreContext, T } from '@/main';
import { IPackage } from '@api/IArchitectApi';
import Button from '@components/Button/Button';
import S from '@components/packages/PackageItem.module.scss';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { VscTrash } from 'react-icons/vsc';

export const PackageItem = observer((props: { package: IPackage; onDelete: () => void }) => {
  const rootStore = useContext(RootStoreContext);
  const packagesState = rootStore.packagesState;
  const isActive = packagesState.activePackageId === props.package.id;
  const isSelected = packagesState.selectedPackageId === props.package.id;

  function onPackageDoubleClick() {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: packagesState.setActivePackageClick(props.package.id),
    });
  }

  return (
    <div
      className={`${S.root} ${isSelected ? S.selected : ''} ${isActive ? S.active : ''}`}
      data-test-id={`package-${props.package.name}`}
      data-selected={isSelected}
      data-active={isActive}
      onClick={() => packagesState.selectPackage(props.package.id)}
      onDoubleClick={onPackageDoubleClick}
    >
      <span className={S.name}>{props.package.name}</span>
      {isSelected && (
        // Keeps the clicks from selecting or activating the row underneath.
        <span
          className={S.deleteButton}
          onClick={event => event.stopPropagation()}
          onDoubleClick={event => event.stopPropagation()}
        >
          <Button
            type="secondary"
            isCompact
            title={T('Delete', 'packages_delete')}
            prefix={<VscTrash />}
            onClick={props.onDelete}
            dataTestId={`package-delete-${props.package.name}`}
          />
        </span>
      )}
    </div>
  );
});
