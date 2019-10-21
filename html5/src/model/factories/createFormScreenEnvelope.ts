
import { FormScreenLifecycle } from '../entities/FormScreenLifecycle/FormScreenLifecycle';
import { FormScreenEnvelope } from 'model/entities/FormScreen';


export function createFormScreenEnvelope() {
  return new FormScreenEnvelope({formScreenLifecycle: new FormScreenLifecycle()});
}