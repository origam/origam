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

import { expect, type APIRequestContext } from '@playwright/test';

export interface AiScriptToolCall {
  name: string;
  arguments?: Record<string, unknown>;
}

export interface AiScriptStep {
  text?: string;
  toolCalls?: AiScriptToolCall[];
  delayMs?: number;
  error?: string;
}

export interface AiScript {
  steps: AiScriptStep[];
  promptTokens?: number;
  completionTokens?: number;
}

export async function setAiScript(request: APIRequestContext, script: AiScript): Promise<void> {
  const response = await request.post('/agent/test/script', { data: script });
  if (!response.ok()) {
    throw new Error(
      `POST /agent/test/script failed: ${response.status()}. The endpoint exists only on a ` +
        'server running with ASPNETCORE_ENVIRONMENT=Development.',
    );
  }
}

export async function clearAiScript(request: APIRequestContext): Promise<void> {
  await request.delete('/agent/test/script');
}

export async function deleteChatThreads(request: APIRequestContext): Promise<void> {
  const threads = await readChatThreads(request);
  for (const thread of threads) {
    await request.post(`/agent/history/delete?threadId=${encodeURIComponent(thread.id)}`);
  }
}

export async function waitForStoredMessages(
  request: APIRequestContext,
  expectedCount: number,
): Promise<void> {
  await expect
    .poll(async () => {
      const threads = await readChatThreads(request);
      return threads.reduce((total, thread) => total + thread.messages.length, 0);
    })
    .toBe(expectedCount);
}

async function readChatThreads(
  request: APIRequestContext,
): Promise<{ id: string; messages: unknown[] }[]> {
  const response = await request.get('/agent/history');
  if (!response.ok()) {
    throw new Error(`GET /agent/history failed: ${response.status()}`);
  }
  return await response.json();
}
