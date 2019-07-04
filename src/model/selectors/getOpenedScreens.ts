import { IOpenedScreens } from '../types/IOpenedScreens';
import { getApplication } from './getApplication';

export function getOpenedScreens(ctx: any): IOpenedScreens {
  return getApplication(ctx).openedScreens;
}