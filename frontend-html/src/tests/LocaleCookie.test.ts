/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { getLocaleFromCookie, initLocaleCookie } from "utils/cookies";
import { getApi } from "model/selectors/getApi";

// what UIService/DefaultLocalizationCookie returns: separators are "|" and one field holds a space
const mockCookieValue =
  "c=fr-CH|uic=fr-CH|defaultDateSeparator=.|defaultTimeSeparator=:|" +
  "defaultDateTimeSeparator= |defaultDateSequence=DayMonthYear|" +
  "defaultLongDateFormat=dd.MM.yyyy HH:mm:ss|defaultShortDateFormat=dd.MM.yyyy|" +
  "defaultTimeFormat=HH:mm:ss";

jest.mock("model/selectors/getApi", () => ({ getApi: jest.fn() }));

const mockedGetApi = getApi as jest.MockedFunction<typeof getApi>;
const api = { defaultLocalizationCookie: async () => mockCookieValue } as any;

function clearCookies() {
  document.cookie
    .split(";")
    .map((cookie) => cookie.split("=")[0].trim())
    .filter((name) => name.length > 0)
    .forEach((name) => {
      document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`;
    });
}

beforeEach(() => {
  clearCookies();
  mockedGetApi.mockReset();
  mockedGetApi.mockReturnValue(api);
});

test("writes the language cookie percent-encoded", async () => {
  await initLocaleCookie({});

  const written = document.cookie.split("origamCurrentLocale=")[1];
  expect(written).toBeDefined();
  // a raw space makes ASP.NET Core's cookie parser drop the value
  expect(written).not.toMatch(/[| ]/);
  expect(written.startsWith("c%3Dfr-CH%7Cuic%3Dfr-CH")).toBe(true);
});

test("the stored cookie yields its culture", async () => {
  await initLocaleCookie({});

  expect(getLocaleFromCookie()).toBe("fr-CH");
});

test("a stored cookie is kept as it is", async () => {
  document.cookie = `origamCurrentLocale=${encodeURIComponent(mockCookieValue)}; path=/`;

  await initLocaleCookie({});

  expect(mockedGetApi).not.toHaveBeenCalled();
});
