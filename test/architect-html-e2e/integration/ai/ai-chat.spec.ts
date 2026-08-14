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

import { expect, test, type Page } from '@playwright/test';
import {
  clearAiScript,
  deleteChatThreads,
  setAiScript,
  waitForStoredMessages,
} from '@support/aiScript';

// Chat history in the AI panel as it is really used - docked inside the full
// Architect UI at "/", next to the model tree and the editors, not the bare
// "/ai" page. The language model never runs: beforeEach queues a scripted
// answer, and while a script is queued the agent answers from it and never
// reaches the live client, so no request leaves the machine for the whole test.
// Everything else is production code: the AG-UI stream, the assembled prompt
// and the /ChatHistory endpoints behind the panel.
//
// The server keeps one queued script at a time, so tests here must not overlap.
test.describe.configure({ mode: 'serial' });

const SCRIPTED_ANSWER = 'The scripted model answered.';

function messagesWithText(page: Page, text: string) {
  return page.locator('[data-test-id="ai-message"]').filter({ hasText: text });
}

function threadOptions(page: Page) {
  return page.getByTestId('ai-thread-select').locator('option');
}

async function sendMessage(page: Page, text: string) {
  await page.getByTestId('ai-input').fill(text);
  await page.getByTestId('ai-send').click();
  await expect(messagesWithText(page, SCRIPTED_ANSWER).last()).toBeVisible();
  await expect(page.getByTestId('ai-send')).toBeVisible();
}

async function openThread(page: Page, title: string) {
  await page.getByTestId('ai-thread-select').selectOption({ label: title });
}

// The panel is docked in the Architect layout and UiState.aiPanelVisible starts
// out true, so a fresh browser context opens with it already showing.
async function openArchitectWithAiPanel(page: Page) {
  await page.goto('/');
  await expect(page.getByTestId('topbar-toggle-ai')).toBeVisible();
  await expect(page.getByTestId('ai-input')).toBeVisible();
}

async function reloadArchitect(page: Page) {
  await page.reload();
  await expect(page.getByTestId('ai-input')).toBeVisible();
}

test.describe('AI chat history', () => {
  test.beforeEach(async ({ request }) => {
    // Stored threads are merged into the panel on load and the first of them
    // becomes the active one, so chats left by an earlier run would show up as
    // the conversation under test.
    await deleteChatThreads(request);
    await setAiScript(request, {
      steps: [{ text: SCRIPTED_ANSWER }],
      promptTokens: 900,
      completionTokens: 12,
    });
  });

  test.afterEach(async ({ request }) => {
    // A script left queued would keep answering in the developer's own chat.
    await clearAiScript(request);
  });

  test('keeps chats separate when switching between them', async ({ page, request }) => {
    await openArchitectWithAiPanel(page);
    await sendMessage(page, 'First chat question');

    await page.getByTestId('ai-new-chat').click();
    await expect(messagesWithText(page, 'First chat question')).toHaveCount(0);
    await sendMessage(page, 'Second chat question');

    await openThread(page, 'First chat question');
    await expect(messagesWithText(page, 'First chat question')).toHaveCount(1);
    await expect(messagesWithText(page, 'Second chat question')).toHaveCount(0);

    await openThread(page, 'Second chat question');
    await expect(messagesWithText(page, 'Second chat question')).toHaveCount(1);
    await expect(messagesWithText(page, 'First chat question')).toHaveCount(0);

    await waitForStoredMessages(request, 4);
    await reloadArchitect(page);
    await expect(threadOptions(page)).toHaveCount(2);

    // Both chats have to come back with their own messages, not merged into one.
    await openThread(page, 'First chat question');
    await expect(messagesWithText(page, 'First chat question')).toHaveCount(1);
    await expect(messagesWithText(page, 'Second chat question')).toHaveCount(0);

    await openThread(page, 'Second chat question');
    await expect(messagesWithText(page, 'Second chat question')).toHaveCount(1);
    await expect(messagesWithText(page, 'First chat question')).toHaveCount(0);
  });
});
