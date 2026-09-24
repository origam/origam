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

import type { APIRequestContext } from '@playwright/test';

export async function activatePackage(
  request: APIRequestContext,
  packageName: string,
): Promise<void> {
  const packagesResponse = await request.get('/Package/GetAll');
  if (!packagesResponse.ok()) {
    throw new Error(
      `GET /Package/GetAll failed: ${packagesResponse.status()} ${await packagesResponse.text()}`,
    );
  }

  const { packages } = (await packagesResponse.json()) as {
    packages: { id: string; name: string }[];
  };
  const targetPackage = packages.find(candidate => candidate.name === packageName);
  if (!targetPackage) {
    throw new Error(`Package "${packageName}" was not found in the test model.`);
  }

  const activateResponse = await request.post('/Package/SetActive', {
    data: { id: targetPackage.id },
  });
  if (!activateResponse.ok()) {
    throw new Error(
      `POST /Package/SetActive failed: ${activateResponse.status()} ${await activateResponse.text()}`,
    );
  }
}
