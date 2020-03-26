
function getCookie(name: string): string {
  var value = "; " + document.cookie;
  var parts = value.split("; " + name + "=");

  if(parts.length == 2){
    const cookieValue = parts.pop()!.split(";").shift()
    return cookieValue ? cookieValue : ""
  }
  return ""
}


export function getLocaleFromCookie(): string {
  const cookieValue = unescape(getCookie("origamCurrentLocale"))
  const pattern = /c=([a-zA-Z\-]+)\|/i 
  const results = cookieValue.match(pattern)
  return results ? results[1] : "cs-CZ"
}