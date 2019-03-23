export function parseBoolean(value: string | undefined) {
  return value === "true";
}

export function parseNumber(value: string | undefined) {
  return value !== undefined ? parseInt(value, 10) : undefined;
}
