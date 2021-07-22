/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

import { createAtom, observable, action } from "mobx";
import { processedImageURL } from "utils/image";

export class ScreenPictureCache {
  @observable items = new Map<string, any>();
  atoms = new Map<string, any>();

  @action.bound
  atomBecameObserved(url: string, atom: any) {
    if (!this.items.has(url)) {
      const procUrl = processedImageURL(url);
      const img = new Image();
      this.items.set(url, img);
      atom.reportChanged();
      img.onload = () => {
        atom.reportChanged();
      };
      img.onerror = () => {
        this.items.set(url, null);
      };
      img.src = procUrl.value!;
    }
  }

  getImage(url: string) {
    if (!this.atoms.has(url)) {
      const atom = createAtom(
        "ImageCacheAtom",
        () => {
          this.atomBecameObserved(url, atom);
        },
        () => {
          this.atoms.delete(url);
        }
      );
      this.atoms.set(url, atom);
    }
    const atom = this.atoms.get(url);
    atom.reportObserved();
    return this.items.get(url)!;
  }
}
