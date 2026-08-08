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

export type AffectedNode = {
  origamId: string;
  label?: string | null;
  itemTypeName?: string | null;
  action: string;
};

export type ChatMessage = {
  id: string;
  role: 'user' | 'assistant';
  text: string;
  calledFunctions?: string[];
  affectedNodes?: AffectedNode[];
  images?: string[];
  totalTokens?: number;
};

export type ChatThread = {
  id: string;
  title: string;
  messages: ChatMessage[];
  createdAt: number;
  tokensUsed: number;
  summary?: string;
};

export type AttachedImage = {
  id: string;
  dataUrl: string;
};

export type FocusItem = {
  label: string;
  itemTypeName?: string;
  origamId?: string;
};

export type FocusNode = {
  label: string;
  itemTypeName?: string;
  origamId?: string;
  path?: string;
};

export type ChatFocus = {
  activeEditor?: FocusItem;
  openTabs?: FocusItem[];
  visibleNodes?: FocusNode[];
};

export type ApiSection = {
  name: string;
  functionCount: number;
  functions: string[];
  hasDestructive: boolean;
};

export type RunUsage = {
  promptTokens: number;
  completionTokens: number;
  totalTokens: number;
  cachedTokens: number;
};

export type RunResult = {
  modelChanged: boolean;
  affectedNodes: AffectedNode[];
  toolLimitReached: boolean;
  updatedSummary?: string | null;
  usage: RunUsage;
};
