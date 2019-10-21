import jwt from "jsonwebtoken";

export function getUserNameFromToken(token: string) {
  const decodedToken = jwt.decode(token);
  const userName =
    decodedToken !== null
      ? (decodedToken as {
          [key: string]: string;
        })["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]
      : undefined;
  return userName;
}
