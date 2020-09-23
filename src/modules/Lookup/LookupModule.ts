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
      console.log("Starting up lookup", lookupId);
      lookupCleanerReloaderById.set(lookupId, lookupCleanerReloader);
      disposers.push(
        multiLookupEngine.lookupLoaderMulti.resultListeners.subscribe(
          lookupLoaderIndividual.handleResultingLabels
        )
      );
    },
    teardown() {
      debugger;
      console.log("Stopping lookup", lookupId);
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
