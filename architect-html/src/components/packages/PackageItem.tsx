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
import { IPackage } from '@api/IArchitectApi';
import S from '@components/packages/PackageItem.module.scss';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { useContext } from 'react';

export function PackageItem(props: { package: IPackage; isSelected?: boolean }) {
  const rootStore = useContext(RootStoreContext);
  const packagesState = rootStore.packagesState;

  async function onPackageClick() {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: packagesState.setActivePackageClick(props.package.id),
    });
  }

  return (
    <div className={S.root + ' ' + (props.isSelected ? S.selected : '')} onClick={onPackageClick}>
      {props.package.name}
    </div>
  );
}
