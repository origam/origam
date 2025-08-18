/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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

export class KeyBuffer {

  constructor(public parent: any) {
    this.start();
  }

  buffer: string = "";
  clearDelayMillis = 2000;
  timeout: undefined | NodeJS.Timeout;

  start() {
    window.document.addEventListener("keydown", (event) => this.onKeyDown(event))
  }

  onKeyDown(event: KeyboardEvent) {
    if (window.document.activeElement?.tagName !== "INPUT" &&
      window.document.activeElement?.tagName !== "TEXTAREA" &&
      this.isCharacterKeyEvent(event))
    {
      this.buffer += event.key;
      if (this.timeout) {
        clearTimeout(this.timeout)
      }
      this.timeout = setTimeout(
        () => {
          this.buffer = "";
        },
        this.clearDelayMillis);
    }
  }

  isCharacterKeyEvent(event: KeyboardEvent) {
    return event.key.length === 1 &&
      !event.ctrlKey &&
      !event.metaKey &&
      !event.altKey
  }

  getAndClear() {
    if (this.timeout) {
      clearTimeout(this.timeout)
    }
    const bufferCopy = this.buffer;
    this.buffer = "";
    return bufferCopy;
  }
}