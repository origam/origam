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

import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import {
  clearAiScript,
  deleteChatThreads,
  setAiScript,
  waitForStoredMessages,
} from '@support/aiScript';

test.describe.configure({ mode: 'serial' });

const TOOL_NAME = 'GetCurrentTime';
const FIRST_ANSWER = 'The scripted model used a tool.';
const SECOND_ANSWER = 'The scripted model answered again.';

interface StoredToolCall {
  id: string;
  name: string;
  arguments?: string;
  result?: string;
}

async function readStoredToolCalls(request: APIRequestContext): Promise<StoredToolCall[]> {
  const response = await request.get('/agent/history');
  expect(response.ok()).toBe(true);
  const threads: { messages: { toolCalls?: StoredToolCall[] }[] }[] = await response.json();
  return threads.flatMap(thread => thread.messages.flatMap(message => message.toolCalls ?? []));
}

async function openArchitectWithAiPanel(page: Page) {
  await page.goto('/');
  await expect(page.getByTestId('topbar-toggle-ai')).toBeVisible();
  await expect(page.getByTestId('ai-input')).toBeVisible();
}

async function sendMessage(page: Page, text: string, expectedAnswer: string) {
  await page.getByTestId('ai-input').fill(text);
  await page.getByTestId('ai-send').click();
  await expect(
    page.locator('[data-test-id="ai-message"]').filter({ hasText: expectedAnswer }).last(),
  ).toBeVisible();
  await expect(page.getByTestId('ai-send')).toBeVisible();
}

test.describe('AI tool call history', () => {
  test.beforeEach(async ({ request }) => {
    await deleteChatThreads(request);
  });

  test.afterEach(async ({ request }) => {
    await clearAiScript(request);
  });

  test('sends the tool calls of an earlier turn back with the next question', async ({
    page,
    request,
  }) => {
    await setAiScript(request, {
      steps: [{ toolCalls: [{ name: TOOL_NAME }] }, { text: FIRST_ANSWER }],
    });

    await openArchitectWithAiPanel(page);
    await sendMessage(page, 'What time is it?', FIRST_ANSWER);

    await waitForStoredMessages(request, 2);
    const storedToolCalls = await readStoredToolCalls(request);
    expect(storedToolCalls).toHaveLength(1);
    expect(storedToolCalls[0].name).toBe(TOOL_NAME);
    expect(storedToolCalls[0].result ?? '').not.toBe('');

    await setAiScript(request, { steps: [{ text: SECOND_ANSWER }] });
    const secondRun = page.waitForRequest(
      request => request.url().includes('/agent/architect') && request.method() === 'POST',
    );
    await sendMessage(page, 'And now?', SECOND_ANSWER);

    const sentMessages: {
      role: string;
      toolCallId?: string;
      toolCalls?: { id: string; function: { name: string } }[];
    }[] = JSON.parse((await secondRun).postData() ?? '{}').messages;

    const assistantMessage = sentMessages.find(message => (message.toolCalls ?? []).length > 0);
    expect(assistantMessage?.toolCalls?.[0].function.name).toBe(TOOL_NAME);
    expect(assistantMessage?.toolCalls?.[0].id).toBe(storedToolCalls[0].id);

    const toolMessage = sentMessages.find(message => message.role === 'tool');
    expect(toolMessage?.toolCallId).toBe(storedToolCalls[0].id);
  });
});
