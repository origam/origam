export enum LabelPosition {Left, Right, Top, Bottom, None}

export function parseLabelPosition(value: string | undefined | null): LabelPosition {
  if (value === undefined || value === null || value === '') {
    return LabelPosition.None;
  }
  const intValue = parseInt(value)
  if (isNaN(intValue)) {
    const validOptions = Object.keys(LabelPosition);
    if (!validOptions.includes(value)) {
      throw new Error(`Invalid LabelPosition: ${value}. Valid values are: ${validOptions.join(', ')}`);
    } else {
      return LabelPosition[value as any] as any;
    }
  }

  const validOptions = Object.values(LabelPosition);
  if (!validOptions.includes(intValue as LabelPosition)) {
    throw new Error(`Invalid LabelPosition: ${value}. Valid values are: ${validOptions.join(', ')}`);
  }

  return intValue as LabelPosition;
}
