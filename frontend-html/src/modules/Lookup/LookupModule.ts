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

import { Container, TypeSymbol } from "dic/Container";
import { ILookupIndividualEngine } from "model/entities/Property";
import { IApi } from "model/entities/types/IApi";
import { Clock } from "./Clock";
import { LookupApi } from "./LookupApi";
import { LookupCacheDependencies } from "./LookupCacheDependencies";
import { LookupCacheIndividual } from "./LookupCacheIndividual";
import { LookupCacheMulti } from "./LookupCacheMulti";
import { LookupLabelsCleanerReloader } from "./LookupCleanerLoader";
import { LookupLoaderIndividual } from "./LookupLoaderIndividual";
import { LookupLoaderMulti } from "./LookupLoaderMulti";
import { LookupResolver } from "./LookupResolver";
import { ILookupScopeRegistry } from "./LookupScopeRegistry";

export const SCOPE_Lookup = "Lookup";

export const ILookupId = TypeSymbol<string>("ILookupId");

export function register($cont: Container) {}

export function createMultiLookupEngine(origamApi: () => IApi): IMultiLookupEngine {
  const lookupCleanerReloaderById = new Map<string, LookupLabelsCleanerReloader>();
  const clock = new Clock();
  const api = new LookupApi(origamApi);
  const lookupCacheMulti = new LookupCacheMulti(
    clock,
    (lookupId) => lookupCleanerReloaderById.get(lookupId)!
  );
  const lookupLoaderMulti = new LookupLoaderMulti(clock, api);
  const lookupEngineById = new Map<string, ILookupIndividualEngine>();

  const cacheDependencies = new LookupCacheDependencies();

  return {
    lookupCacheMulti,
    lookupLoaderMulti,
    lookupCleanerReloaderById,
    lookupEngineById,
    cacheDependencies,
    startup() {
      lookupCacheMulti.startup();
    },
    teardown() {
      lookupCacheMulti.teardown();
    },
  };
}

export interface IMultiLookupEngine {
  lookupCacheMulti: LookupCacheMulti;
  lookupLoaderMulti: LookupLoaderMulti;
  lookupCleanerReloaderById: Map<string, LookupLabelsCleanerReloader>;
  lookupEngineById: Map<string, ILookupIndividualEngine>;
  cacheDependencies: LookupCacheDependencies;
  startup(): void;
  teardown(): void;
}

export function createIndividualLookupEngine(
  lookupId: string,
  multiLookupEngine: IMultiLookupEngine
) {
  const { lookupCacheMulti, lookupLoaderMulti, lookupCleanerReloaderById } = multiLookupEngine;
  const lookupCacheIndividual = new LookupCacheIndividual(lookupId, lookupCacheMulti);
  const lookupLoaderIndividual = new LookupLoaderIndividual(lookupId, lookupLoaderMulti);
  const lookupResolver = new LookupResolver(lookupCacheIndividual, lookupLoaderIndividual);
  const lookupCleanerReloader = new LookupLabelsCleanerReloader(
    lookupCacheIndividual,
    lookupResolver
  );

  lookupLoaderIndividual.resultListeners.subscribe(lookupResolver.handleResultingLabels);
  const disposers: any[] = [];
  return {
    lookupResolver,
    lookupCleanerReloader,
    startup() {
      lookupCleanerReloaderById.set(lookupId, lookupCleanerReloader);
      disposers.push(
        multiLookupEngine.lookupLoaderMulti.resultListeners.subscribe(
          lookupLoaderIndividual.handleResultingLabels
        )
      );
    },
    teardown() {
      lookupCleanerReloaderById.delete(lookupId);
      for (let d of disposers) d();
    },
    cleanAndReload() {
      lookupCleanerReloader.reloadLookupLabels();
    },
  };
}

export function beginLookupScope($parent: Container, lookupId: string) {
  const $cont = $parent.beginLifetimeScope(SCOPE_Lookup);
  $cont.register(ILookupId, () => lookupId).scopedInstance(SCOPE_Lookup);
  $cont.resolve(ILookupScopeRegistry).addScope($cont);
  $cont.onThisScopeWillDispose(($cont) => $cont.resolve(ILookupScopeRegistry).removeScope($cont));
  return $cont;
}
