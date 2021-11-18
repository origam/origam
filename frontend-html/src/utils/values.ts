export function isBase64(str?: string) {
  if (!str) return false;
  return str.search(/^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/) > -1;
}
