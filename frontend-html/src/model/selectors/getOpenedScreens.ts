import {IOpenedScreens} from '../entities/types/IOpenedScreens';
import {getWorkbench} from './getWorkbench';

export function getOpenedScreens(ctx: any): IOpenedScreens {
  return getWorkbench(ctx).openedScreens;
}