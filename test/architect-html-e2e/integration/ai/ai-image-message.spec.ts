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

import { expect, test } from '@playwright/test';

const PIXEL_PNG = Buffer.from(
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==',
  'base64',
);

interface AgentInput {
  threadId: string;
  messages: { role: string; content: unknown }[];
}

test('an image sent without text reaches the agent with no question added', async ({
  page,
  request,
}) => {
  let agentInput: AgentInput | undefined;
  await page.route('**/agent/architect', async route => {
    agentInput = route.request().postDataJSON() as AgentInput;
    await route.abort();
  });
  await page.goto('/');
  await expect(page.getByTestId('ai-input')).toBeVisible();
  await page.getByTestId('ai-new-chat').click();

  await page
    .locator('input[type="file"][accept="image/*"]')
    .setInputFiles({ name: 'pixel.png', mimeType: 'image/png', buffer: PIXEL_PNG });
  await page.getByTestId('ai-send').click();
  await expect.poll(() => agentInput).toBeDefined();
  await expect(page.getByTestId('ai-send')).toBeVisible();

  try {
    expect(agentInput!.messages.at(-1)).toEqual({
      id: expect.any(String),
      role: 'user',
      content: [{ type: 'binary', mimeType: 'image/png', data: PIXEL_PNG.toString('base64') }],
    });
    await expect(
      page.getByTestId('ai-message').filter({ hasText: 'What is in this image?' }),
    ).toHaveCount(0);
  } finally {
    await request.post(
      `/agent/history/delete?threadId=${encodeURIComponent(agentInput!.threadId)}`,
    );
  }
});
