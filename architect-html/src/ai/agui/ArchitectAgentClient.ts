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

import { ChatFocus, ChatMessage, RunResult } from '@/ai/AiAgentTypes';
import { HttpAgent, InputContent, Message } from '@ag-ui/client';

const ARCHITECT_AGENT_ENDPOINT = '/agent/architect';

export const RUN_RESULT_EVENT_NAME = 'origam.agent.runResult';

const DATA_URL_PATTERN = /^data:([^;,]+);base64,(.+)$/;

export function createArchitectAgent(options: {
  threadId: string;
  messages: ChatMessage[];
  focus: ChatFocus;
}): HttpAgent {
  return new HttpAgent({
    url: ARCHITECT_AGENT_ENDPOINT,
    threadId: options.threadId,
    initialMessages: toAguiMessages(options.messages),
    initialState: { focus: options.focus },
  });
}

function toAguiMessages(messages: ChatMessage[]): Message[] {
  const converted: Message[] = [];

  for (const message of messages) {
    if (message.role === 'assistant') {
      const answeredCalls = (message.toolCalls ?? []).filter(
        call => call.result !== undefined && call.name.length > 0,
      );
      if (message.text.trim().length === 0 && answeredCalls.length === 0) {
        continue;
      }
      converted.push({
        id: message.id,
        role: 'assistant',
        content: message.text,
        ...(answeredCalls.length > 0 && {
          toolCalls: answeredCalls.map(call => ({
            id: call.id,
            type: 'function' as const,
            function: { name: call.name, arguments: call.arguments || '{}' },
          })),
        }),
      });
      for (const call of answeredCalls) {
        converted.push({
          id: `${message.id}-${call.id}`,
          role: 'tool',
          content: call.result ?? '',
          toolCallId: call.id,
        });
      }
      continue;
    }

    const imageParts = (message.images ?? [])
      .map(image => (image.dataUrl ? toBinaryInputContent(image.dataUrl) : null))
      .filter((part): part is InputContent => part !== null);

    if (imageParts.length === 0) {
      converted.push({ id: message.id, role: 'user', content: message.text });
      continue;
    }

    converted.push({
      id: message.id,
      role: 'user',
      content: [{ type: 'text', text: message.text }, ...imageParts],
    });
  }

  return converted;
}

export function parseRunResult(value: unknown): RunResult | null {
  if (typeof value !== 'object' || value === null) {
    return null;
  }

  const payload = value as Partial<RunResult>;
  const usage = payload.usage;

  return {
    modelChanged: payload.modelChanged === true,
    affectedNodes: Array.isArray(payload.affectedNodes) ? payload.affectedNodes : [],
    toolLimitReached: payload.toolLimitReached === true,
    updatedSummary:
      typeof payload.updatedSummary === 'string' && payload.updatedSummary.trim().length > 0
        ? payload.updatedSummary
        : null,
    usage: {
      promptTokens: usage?.promptTokens ?? 0,
      completionTokens: usage?.completionTokens ?? 0,
      totalTokens: usage?.totalTokens ?? 0,
      cachedTokens: usage?.cachedTokens ?? 0,
    },
  };
}

function toBinaryInputContent(dataUrl: string): InputContent | null {
  const match = DATA_URL_PATTERN.exec(dataUrl);
  if (!match) {
    return null;
  }

  return { type: 'binary', mimeType: match[1], data: match[2] };
}
