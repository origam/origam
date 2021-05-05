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

export class PubSub<T> {
  idGen = 1;
  handlers = new Map<number, (args: T) => void>();

  subscribe(fn: (args: T) => void) {
    const myId = this.idGen++;
    this.handlers.set(myId, fn);
    return () => this.handlers.delete(myId);
  }

  trigger(args: T) {
    for (let h of this.handlers.values()) h(args);
  }
}

export const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export type InterfaceOf<C> = {
  [K in keyof C]: C[K]
}