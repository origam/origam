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

import { simpleErrorHandler } from '@api/ArchitectApi';
import { HttpClient } from '@api/httpClient';

const httpClient = new HttpClient(simpleErrorHandler);

export async function getCustomInstructions(): Promise<string> {
  return (await httpClient.get<{ text: string }>('/agent/prompt/custom')).data.text;
}

export async function saveCustomInstructions(text: string): Promise<void> {
  await httpClient.post('/agent/prompt/custom', { text });
}

export interface AgentConnection {
  model: string;
  router: string;
  hasApiKey: boolean;
}

export async function getAgentConnection(): Promise<AgentConnection> {
  return (await httpClient.get<AgentConnection>('/agent/health')).data;
}
