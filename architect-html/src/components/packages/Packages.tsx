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
import { hasInvalidFileNameChars, isReservedOrUnsafeFileName } from '@/utils/fileNameRules';
import { IPackage, IPackageReferencesInfo } from '@api/IArchitectApi';
import Button from '@components/Button/Button';
import { PackageItem } from '@components/packages/PackageItem';
import { askForPackageReferences } from '@components/packages/PackageReferencesDialog';
import S from '@components/packages/Packages.module.scss';
import { askForName, askYesNoQuestion, YesNoResult } from '@dialogs/DialogUtils';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import React, { useContext, useEffect } from 'react';
import { VscAdd } from 'react-icons/vsc';

export const Packages: React.FC = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const packagesState = rootStore.packagesState;
  const run = runInFlowWithHandler(rootStore.errorDialogController);

  useEffect(() => {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: packagesState.loadPackages.bind(packagesState),
    });
  }, [packagesState, rootStore.errorDialogController]);

  function validatePackageName(value: string): string | null {
    const name = value.trim();
    if (name.length === 0) {
      return T('Package name cannot be empty.', 'packages_error_name_empty');
    }
    if (hasInvalidFileNameChars(name)) {
      return T('Package name contains invalid characters.', 'packages_error_name_invalid_chars');
    }
    if (isReservedOrUnsafeFileName(name)) {
      return T('Package name is reserved or not allowed.', 'packages_error_name_reserved');
    }
    const taken = packagesState.packages.some(
      existing => existing.name.trim().toLowerCase() === name.toLowerCase(),
    );
    if (taken) {
      return T('A package with this name already exists.', 'packages_error_name_duplicate');
    }
    return null;
  }

  function createPackage() {
    run({
      generator: function* () {
        const name = (yield askForName(rootStore.dialogStack, {
          title: T('New Package', 'packages_new_title'),
          label: T('Package name', 'packages_new_label'),
          validate: validatePackageName,
        })) as string | null;
        if (!name) {
          return;
        }
        yield* packagesState.createPackage(name)();
      },
    });
  }

  function deletePackage(pkg: IPackage) {
    run({
      generator: function* () {
        const answer = (yield askYesNoQuestion(
          rootStore.dialogStack,
          T('Delete Package', 'packages_delete_title'),
          T(
            'Do you really want to delete the package "{0}"? Its folder will be removed from the model.',
            'packages_delete_question',
            pkg.name,
          ),
        )) as YesNoResult;
        if (answer !== YesNoResult.Yes) {
          return;
        }
        yield* packagesState.deletePackage(pkg.id)();
      },
    });
  }

  function editReferences(pkg: IPackage) {
    run({
      generator: function* () {
        const referencesInfo =
          (yield rootStore.architectApi.getPackageReferences()) as IPackageReferencesInfo;
        const referencedPackageIds = (yield askForPackageReferences(
          rootStore.dialogStack,
          pkg.name,
          referencesInfo.candidates,
        )) as string[] | null;
        if (!referencedPackageIds) {
          return;
        }
        yield* packagesState.updateReferences(referencesInfo.packageId, referencedPackageIds)();
      },
    });
  }

  return (
    <div className={S.root}>
      <div className={S.toolbar}>
        <Button
          type="secondary"
          title={T('New Package', 'packages_add')}
          prefix={<VscAdd />}
          onClick={createPackage}
          dataTestId="packages-add"
        />
      </div>
      {packagesState.packages.map(x => (
        <PackageItem
          key={x.id}
          package={x}
          onDelete={() => deletePackage(x)}
          onEditReferences={() => editReferences(x)}
        />
      ))}
    </div>
  );
});
