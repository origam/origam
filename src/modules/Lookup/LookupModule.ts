import { Container, TypeSymbol } from "dic/Container";
import { IClock, Clock } from "./Clock";
import { ILookupApi, LookupApi } from "./LookupApi";
import { ILookupCacheIndividual, LookupCacheIndividual } from "./LookupCacheIndividual";
import { ILookupCacheMulti, LookupCacheMulti } from "./LookupCacheMulti";
import {
  ILookupLabelsCleanerReloader,
  LookupLabelsCleanerReloader,
  IGetLookupLabelsCleanerReloader,
} from "./LookupCleanerLoader";
import { ILookupLoaderIndividual, LookupLoaderIndividual } from "./LookupLoaderIndividual";
import { ILookupLoaderMulti, LookupLoaderMulti } from "./LookupLoaderMulti";
import { ILookupResolver, LookupResolver } from "./LookupResolver";
import { ILookupScopeRegistry, LookupScopeRegistry } from "./LookupScopeRegistry";
import { SCOPE_Workbench } from "modules/Workbench/WorkbenchModule";
import { ILookupListCacheMulti, LookupListCacheMulti } from "./LookupListCacheMulti";
import { IApi } from "model/entities/types/IApi";
import { lookup } from "dns";

export const SCOPE_Lookup = "Lookup";

export const ILookupId = TypeSymbol<string>("ILookupId");

export function register($cont: Container) {
 /* $cont
    .registerClass(ILookupCacheMulti, LookupCacheMulti)
    .scopedInstance(SCOPE_Workbench)
    .onActivated((args) => {
      args.instance.startup();
    })
    .onRelease((args) => {
      args.instance.teardown();
    });*/

  /*$cont.registerClass(ILookupLoaderMulti, LookupLoaderMulti).scopedInstance(SCOPE_Workbench);
  $cont.registerClass(ILookupScopeRegistry, LookupScopeRegistry).scopedInstance(SCOPE_Workbench);

  $cont.registerClass(IClock, Clock).scopedInstance(SCOPE_Workbench);*/

  /*$cont
    .registerClass(ILookupListCacheMulti, LookupListCacheMulti)
    .scopedInstance(SCOPE_Workbench)
    .onActivated((args) => {
      args.instance.startup();
    })
    .onRelease((args) => {
      args.instance.teardown();
    });*/

  /*$cont.registerClass(ILookupApi, LookupApi).scopedInstance(SCOPE_Lookup);
  $cont.registerClass(ILookupCacheIndividual, LookupCacheIndividual).scopedInstance(SCOPE_Lookup);
  $cont.registerClass(ILookupLabelsCleanerReloader, LookupLabelsCleanerReloader).scopedInstance(SCOPE_Lookup);*/
  /*$cont
    .register(IGetLookupLabelsCleanerReloader, undefined, ($cont) => (id: string) =>
      $cont.resolve(ILookupScopeRegistry).getScope(id).resolve(ILookupLabelsCleanerReloader)
    )
    .scopedInstance(SCOPE_Lookup);*/
  /*$cont
    .registerClass(ILookupLoaderIndividual, LookupLoaderIndividual)
    .scopedInstance(SCOPE_Lookup)
    .forward((reg) => {
      let disposer: any;
      reg.onActivating((args) => {
        disposer = ILookupLoaderMulti().resultListeners.subscribe(args.instance.handleResultingLabels);
      });
      reg.onRelease((args) => {
        disposer?.();
      });
      return reg;
    });*/

  /*$cont
    .registerClass(ILookupResolver, LookupResolver)
    .scopedInstance(SCOPE_Lookup)
    .onActivating((args) => {
      ILookupLoaderIndividual().resultListeners.subscribe(args.instance.handleResultingLabels);
    });*/
}

export function createMultiLookupEngine(origamApi: IApi): IMultiLookupEngine {
  const lookupCleanerReloaderById = new Map<string, LookupLabelsCleanerReloader>();
  const clock = new Clock();
  const api = new LookupApi(origamApi);
  const lookupCacheMulti = new LookupCacheMulti(clock, (lookupId) => lookupCleanerReloaderById.get(lookupId)!);
  const lookupLoaderMulti = new LookupLoaderMulti(clock, api);
  return {
    lookupCacheMulti,
    lookupLoaderMulti,
    lookupCleanerReloaderById,
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
  startup(): void;
  teardown(): void;
}

export function createIndividualLookupEngine(lookupId: string, multiLookupEngine: IMultiLookupEngine) {
  const { lookupCacheMulti, lookupLoaderMulti, lookupCleanerReloaderById } = multiLookupEngine;
  const lookupCacheIndividual = new LookupCacheIndividual(lookupId, lookupCacheMulti);
  const lookupLoaderIndividual = new LookupLoaderIndividual(lookupId, lookupLoaderMulti);
  const lookupResolver = new LookupResolver(lookupCacheIndividual, lookupLoaderIndividual);
  const lookupCleanerReloader = new LookupLabelsCleanerReloader(lookupCacheIndividual, lookupResolver);

  lookupLoaderIndividual.resultListeners.subscribe(lookupResolver.handleResultingLabels);
  return {
    lookupResolver,
    lookupCleanerReloader,
    startup() {
      lookupCleanerReloaderById.set(lookupId, lookupCleanerReloader);
    },
    teardown() {
      lookupCleanerReloaderById.delete(lookupId);
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
