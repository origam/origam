import {
  EditorProperty
} from "src/components/editors/gridEditor/EditorProperty.ts";
import { useEffect, useRef, useState } from "react";

const debounceMs = 300;

export const NumericPropertyInput: React.FC<{
  property: EditorProperty;
  onChange: (value: number | null) => void;
  type: "integer" | "float";
}> = ({
        property,
        onChange,
        type,
      }) => {
  const [inputValue, setInputValue] = useState<string>(
    property.value != null ? String(property.value) : ""
  );
  const parseValue = (val: string): number | null => {
    if (val.trim() === "") {
      return null;
    }
    return type === "integer"
      ? parseInt(val, 10)
      : parseFloat(val);
  };

  useEffect(() => {
    setInputValue(property.value != null ? String(property.value) : "");
  }, [property.value]);

  const debounceRef = useRef<number>();

  function onValueChange(value: string) {
    setInputValue(value)
    window.clearTimeout(debounceRef.current);
    debounceRef.current = window.setTimeout(() => {
      onChange(parseValue(value));
    }, debounceMs);
  }

  return (
    <input
      type="number"
      step={type === "float" ? "any" : "1"}
      disabled={property.readOnly}
      value={inputValue}
      onChange={(e) => onValueChange(e.target.value)}
      onBlur={() => {
        if (inputValue.trim() === "") {
          setInputValue(property.value != null ? String(property.value) : "");
        } else {
          onChange(parseValue(inputValue));
        }
      }}
    />
  );
};