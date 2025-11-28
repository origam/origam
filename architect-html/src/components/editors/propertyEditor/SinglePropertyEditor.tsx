import { observer } from 'mobx-react-lite';
import { EditorProperty } from '@editors/gridEditor/EditorProperty.ts';
import { IPropertyManager } from '@editors/propertyEditor/IPropertyManager.tsx';
import { useContext } from 'react';
import { RootStoreContext } from '@/main.tsx';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler.ts';
import S from '@editors/propertyEditor/PropertyEditor.module.scss';
import { NumericPropertyInput } from '@editors/propertyEditor/NumericPropertyInput.tsx';

const SinglePropertyEditor = observer(
  (props: { property: EditorProperty; propertyManager: IPropertyManager; compact?: boolean }) => {
    const rootStore = useContext(RootStoreContext);

    function onValueChange(property: EditorProperty, value: any) {
      runInFlowWithHandler(rootStore.errorDialogController)({
        generator: function* () {
          const parsedValue = property.type === 'enum' ? parseInt(value) : value;
          yield* props.propertyManager.onPropertyUpdated(property, parsedValue);
        },
      });
    }

    function renderPropertyEditor(property: EditorProperty) {
      if (property.type === 'enum' || property.type === 'looukup') {
        return (
          <select
            value={property.value ?? ''}
            onChange={e => onValueChange(property, e.target.value)}
          >
            {property.dropDownValues.map(x => (
              <option key={property.value + x.name} value={x.value}>
                {x.name}
              </option>
            ))}
          </select>
        );
      }
      if (property.type === 'boolean') {
        return (
          <div className={S.checkboxContainer}>
            <input
              type="checkbox"
              checked={property.value}
              onChange={e => onValueChange(property, e.target.checked)}
              disabled={property.readOnly}
              className={S.checkbox}
            />
          </div>
        );
      }
      if (property.type === 'integer' || property.type === 'float') {
        return (
          <NumericPropertyInput
            property={property}
            type={property.type}
            onChange={value => onValueChange(property, value)}
          />
        );
      }
      return (
        <input
          type="text"
          disabled={property.readOnly}
          value={property.value != null ? property.value : undefined}
          onChange={e => onValueChange(property, e.target.value)}
        />
      );
    }

    return renderPropertyEditor(props.property);
  },
);

export default SinglePropertyEditor;
