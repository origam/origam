import * as FormScreenModule from './FormScreen/FormScreenModule';
import { Container } from 'dic/Container';

export const SCOPE_Screen = "Screen";

export function register($cont: Container) {
  FormScreenModule.register($cont);
}