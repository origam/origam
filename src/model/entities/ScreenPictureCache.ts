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
