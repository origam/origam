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

export interface HttpResponse<T> {
  data: T;
  status: number;
}

export interface HttpRequestConfig {
  params?: Record<string, unknown>;
}

export class HttpError extends Error {
  code?: string;
  status?: number;
  response?: { data: any; status: number };
}

function buildUrl(url: string, params?: Record<string, unknown>): string {
  if (!params) return url;
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null) continue;
    if (Array.isArray(value)) {
      for (const item of value) {
        if (item === undefined || item === null) continue;
        search.append(`${key}[]`, String(item));
      }
    } else {
      search.append(key, String(value));
    }
  }
  const query = search.toString();
  if (!query) return url;
  return url + (url.includes('?') ? '&' : '?') + query;
}

async function parseBody(response: Response): Promise<any> {
  if (response.status === 204) return undefined;
  const contentType = response.headers.get('content-type') ?? '';
  const text = await response.text();
  if (text === '') return undefined;
  if (contentType.includes('application/json')) {
    try {
      return JSON.parse(text);
    } catch {
      return text;
    }
  }
  return text;
}

export class HttpClient {
  constructor(private onError: (error: HttpError) => void) {}

  get<T = any>(url: string, config?: HttpRequestConfig): Promise<HttpResponse<T>> {
    return this.request<T>('GET', url, config);
  }

  post<T = any>(url: string, body?: unknown): Promise<HttpResponse<T>> {
    return this.request<T>('POST', url, undefined, body);
  }

  private async request<T>(
    method: 'GET' | 'POST',
    url: string,
    config?: HttpRequestConfig,
    body?: unknown,
  ): Promise<HttpResponse<T>> {
    const finalUrl = buildUrl(url, config?.params);
    const init: RequestInit = {
      method,
      credentials: 'same-origin',
    };
    if (body !== undefined) {
      init.headers = { 'Content-Type': 'application/json' };
      init.body = JSON.stringify(body);
    }

    let response: Response;
    try {
      response = await fetch(finalUrl, init);
    } catch (e: any) {
      if (e?.name === 'AbortError') throw e;
      const err = new HttpError(e?.message ?? 'Network Error');
      err.code = 'ERR_NETWORK';
      this.onError(err);
      throw err;
    }

    const data = await parseBody(response);

    if (!response.ok) {
      const err = new HttpError(`Request failed with status ${response.status}`);
      err.status = response.status;
      err.response = { data, status: response.status };
      this.onError(err);
      throw err;
    }

    return { data: data as T, status: response.status };
  }
}
