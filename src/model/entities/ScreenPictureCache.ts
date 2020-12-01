import { createAtom, observable } from "mobx";

export class ScreenPictureCache {
  @observable items = new Map<string, any>();
  atoms = new Map<string, any>();

  atomBecameObserved(url: string, atom: any) {
    if (!this.items.has(url)) {
      const img = new Image();
      this.items.set(url, img);
      atom.reportChanged();
      img.onload = () => {
        atom.reportChanged();
      };
      img.onerror = () => {
        this.items.set(url, null);
      };
      img.src = url;
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
