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

import { ChatThread } from '@/ai/AiAgentTypes';
import { simpleErrorHandler } from '@api/ArchitectApi';
import { HttpClient } from '@api/httpClient';

const httpClient = new HttpClient(simpleErrorHandler);

export async function getChatThreads(): Promise<ChatThread[]> {
  return (await httpClient.get<ChatThread[]>('/agent/history')).data;
}

export async function saveChatThread(thread: ChatThread): Promise<void> {
  await httpClient.post('/agent/history/save', thread);
}

export async function deleteChatThread(threadId: string): Promise<void> {
  await httpClient.post('/agent/history/delete', { id: threadId });
}

export async function saveChatImage(
  threadId: string,
  imageId: string,
  mimeType: string,
  dataUrl: string,
): Promise<void> {
  await httpClient.post('/agent/history/image', {
    threadId,
    imageId,
    mimeType,
    data: stripDataUrlPrefix(dataUrl),
  });
}

export function chatImageUrl(threadId: string, imageId: string): string {
  return `/agent/history/image?threadId=${encodeURIComponent(threadId)}&imageId=${encodeURIComponent(imageId)}`;
}

export async function fetchChatImageAsDataUrl(threadId: string, imageId: string): Promise<string> {
  const response = await fetch(chatImageUrl(threadId, imageId));
  if (!response.ok) {
    throw new Error(`Chat image request failed with status ${response.status}`);
  }
  return blobToDataUrl(await response.blob());
}

export function blobToDataUrl(blob: Blob): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = () => reject(reader.error);
    reader.readAsDataURL(blob);
  });
}

function stripDataUrlPrefix(dataUrl: string): string {
  const separatorIndex = dataUrl.indexOf(',');
  return separatorIndex >= 0 ? dataUrl.slice(separatorIndex + 1) : dataUrl;
}
