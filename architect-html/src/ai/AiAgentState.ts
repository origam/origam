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

import {
  AffectedNode,
  ApiSection,
  AttachedImage,
  ChatFocus,
  ChatMessage,
  ChatThread,
  FocusItem,
  FocusNode,
} from '@/ai/AiAgentTypes';
import {
  createArchitectAgent,
  parseRunResult,
  RUN_RESULT_EVENT_NAME,
} from '@/ai/agui/ArchitectAgentClient';
import { T } from '@/main';
import { AgentSubscriber, HttpAgent } from '@ag-ui/client';
import { TreeNode } from '@components/modelTree/TreeNode';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { RootStore } from '@stores/RootStore';
import { action, observable, toJS } from 'mobx';

const THREADS_STORAGE_KEY = 'origam-ai-threads';
const ENABLED_SECTIONS_STORAGE_KEY = 'origam-ai-enabled-sections-v2';
const SAFE_DEFAULT_SECTIONS = [
  'Wizard',
  'Search',
  'Documentation',
  'Tab',
  'Model',
  'PropertyEditor',
  'SectionEditor',
];
const MAX_VISIBLE_NODES = 40;
const GUID_PATTERN =
  /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

export class AiAgentState {
  @observable accessor threads: ChatThread[] = [];
  @observable accessor activeThreadId: string = '';
  @observable accessor draft: string = '';
  @observable accessor attachedImages: AttachedImage[] = [];
  @observable accessor isRunning: boolean = false;
  @observable accessor errorText: string | null = null;
  @observable accessor sections: ApiSection[] = [];
  @observable accessor sectionsAvailable: boolean = true;
  @observable accessor enabledSections: string[] = [];

  private runningAgent: HttpAgent | null = null;
  private isAborting: boolean = false;
  private streamedMessageIds = new Map<string, string>();

  constructor(private rootStore: RootStore) {
    const storedThreads = loadStoredThreads();
    this.threads =
      storedThreads.length > 0 ? storedThreads : [createThread(this.defaultThreadTitle)];
    this.activeThreadId = this.threads[0].id;
    this.enabledSections = loadEnabledSections();
  }

  get activeThread(): ChatThread {
    return this.threads.find(thread => thread.id === this.activeThreadId) ?? this.threads[0];
  }

  get messages(): ChatMessage[] {
    return this.activeThread.messages;
  }

  get canSend(): boolean {
    return !this.isRunning && (this.draft.trim().length > 0 || this.attachedImages.length > 0);
  }

  private get defaultThreadTitle(): string {
    return T('New chat', 'ai_chat_thread_default');
  }

  async loadSections() {
    try {
      const response = await fetch('/agent/architect/sections');
      const payload = await response.json();
      this.applySections(payload.available !== false, payload.sections);
    } catch {
      this.applySections(false, []);
    }
  }

  @action
  private applySections(available: boolean, sections: unknown) {
    this.sectionsAvailable = available;
    this.sections = Array.isArray(sections) ? (sections as ApiSection[]) : [];
  }

  @action
  selectThread(threadId: string) {
    this.activeThreadId = threadId;
  }

  @action
  createNewThread() {
    const thread = createThread(this.defaultThreadTitle);
    this.threads = [thread, ...this.threads];
    this.activeThreadId = thread.id;
    this.resetComposer();
    this.persistThreads();
  }

  @action
  deleteActiveThread() {
    const remaining = this.threads.filter(thread => thread.id !== this.activeThreadId);
    this.threads = remaining.length > 0 ? remaining : [createThread(this.defaultThreadTitle)];
    this.activeThreadId = this.threads[0].id;
    this.resetComposer();
    this.persistThreads();
  }

  @action
  setDraft(draft: string) {
    this.draft = draft;
  }

  @action
  toggleSection(sectionName: string) {
    this.enabledSections = this.enabledSections.includes(sectionName)
      ? this.enabledSections.filter(name => name !== sectionName)
      : [...this.enabledSections, sectionName];
    try {
      localStorage.setItem(ENABLED_SECTIONS_STORAGE_KEY, JSON.stringify(this.enabledSections));
    } catch {
      return;
    }
  }

  async addImageFiles(files: FileList | File[] | null) {
    if (!files) {
      return;
    }
    const imageFiles = Array.from(files).filter(file => file.type.startsWith('image/'));
    const loaded = await Promise.all(
      imageFiles.map(async file => ({
        id: crypto.randomUUID(),
        dataUrl: await readFileAsDataUrl(file),
      })),
    );
    this.appendAttachedImages(loaded);
  }

  @action
  private appendAttachedImages(images: AttachedImage[]) {
    if (images.length > 0) {
      this.attachedImages = [...this.attachedImages, ...images];
    }
  }

  @action
  removeAttachedImage(imageId: string) {
    this.attachedImages = this.attachedImages.filter(image => image.id !== imageId);
  }

  async send() {
    if (!this.canSend) {
      return;
    }

    const thread = this.activeThread;
    const messageText =
      this.draft.trim() || T('What is in this image?', 'ai_chat_default_image_prompt');
    const images = this.attachedImages.map(image => image.dataUrl);

    this.startRun(thread, messageText, images);

    const agent = createArchitectAgent({
      threadId: thread.id,
      messages: toJS(thread.messages),
      focus: buildChatFocus(this.rootStore),
    });
    this.runningAgent = agent;

    try {
      await agent.runAgent(
        {
          forwardedProps: {
            enabledSections: [...this.enabledSections],
            summary: thread.summary,
          },
        },
        this.buildSubscriber(thread.id),
      );
    } catch (error) {
      this.reportRunError(error);
    } finally {
      this.finishRun();
    }
  }

  stop() {
    this.isAborting = true;
    this.runningAgent?.abortRun();
  }

  openAffectedNode(node: AffectedNode) {
    if (node.action === 'deleted') {
      return;
    }
    const run = runInFlowWithHandler(this.rootStore.errorDialogController);
    const rootStore = this.rootStore;
    run({
      generator: function* () {
        yield* rootStore.editorTabViewState.openEditorByOrigamId(node.origamId)();
        rootStore.modelTreeState.highlightNode(node.origamId);
      },
    });
  }

  private buildSubscriber(threadId: string): AgentSubscriber {
    return {
      onTextMessageStartEvent: ({ event }) => {
        this.beginAssistantMessage(threadId, event.messageId);
      },
      onTextMessageContentEvent: ({ event }) => {
        this.appendAssistantText(threadId, event.messageId, event.delta);
      },
      onToolCallStartEvent: ({ event }) => {
        this.addCalledFunction(threadId, event.toolCallName);
      },
      onCustomEvent: ({ event }) => {
        if (event.name === RUN_RESULT_EVENT_NAME) {
          this.applyRunResult(threadId, event.value);
        }
      },
      onRunErrorEvent: ({ event }) => {
        this.setErrorText(event.message);
      },
    };
  }

  @action
  private startRun(thread: ChatThread, messageText: string, images: string[]) {
    if (thread.messages.length === 0 && this.draft.trim().length > 0) {
      thread.title = messageText.slice(0, 40);
    }
    thread.messages.push({
      id: crypto.randomUUID(),
      role: 'user',
      text: messageText,
      images: images.length > 0 ? images : undefined,
    });
    this.streamedMessageIds.clear();
    this.errorText = null;
    this.isAborting = false;
    this.isRunning = true;
    this.resetComposer();
    this.persistThreads();
  }

  @action
  private finishRun() {
    this.isRunning = false;
    this.isAborting = false;
    this.runningAgent = null;
    this.persistThreads();
  }

  @action
  private reportRunError(error: unknown) {
    if (this.isAborting || (error as Error)?.name === 'AbortError') {
      return;
    }
    this.errorText = T(
      'Something went wrong. Is the AI service running on port 5210?',
      'ai_chat_error',
    );
  }

  @action
  private setErrorText(message: string | undefined) {
    this.errorText =
      message && message.trim().length > 0
        ? message
        : T('Something went wrong. Is the AI service running on port 5210?', 'ai_chat_error');
  }

  @action
  private beginAssistantMessage(threadId: string, streamedMessageId: string) {
    const thread = this.findThread(threadId);
    if (!thread || this.streamedMessageIds.has(streamedMessageId)) {
      return;
    }
    const pending = this.pendingAssistantMessage(thread);
    if (pending && pending.text.length === 0) {
      this.streamedMessageIds.set(streamedMessageId, pending.id);
      return;
    }
    const message: ChatMessage = { id: crypto.randomUUID(), role: 'assistant', text: '' };
    thread.messages.push(message);
    this.streamedMessageIds.set(streamedMessageId, message.id);
  }

  @action
  private appendAssistantText(threadId: string, streamedMessageId: string, delta: string) {
    const thread = this.findThread(threadId);
    if (!thread) {
      return;
    }
    this.beginAssistantMessage(threadId, streamedMessageId);
    const messageId = this.streamedMessageIds.get(streamedMessageId);
    const message = thread.messages.find(candidate => candidate.id === messageId);
    if (message) {
      message.text += delta;
    }
  }

  @action
  private addCalledFunction(threadId: string, functionName: string) {
    const thread = this.findThread(threadId);
    if (!thread) {
      return;
    }
    let message = this.pendingAssistantMessage(thread);
    if (!message) {
      message = { id: crypto.randomUUID(), role: 'assistant', text: '' };
      thread.messages.push(message);
    }
    message.calledFunctions = [...(message.calledFunctions ?? []), functionName];
  }

  @action
  private applyRunResult(threadId: string, value: unknown) {
    const thread = this.findThread(threadId);
    const result = parseRunResult(value);
    if (!thread || !result) {
      return;
    }

    const message = this.pendingAssistantMessage(thread);
    if (message) {
      message.totalTokens = result.usage.totalTokens;
      if (result.affectedNodes.length > 0) {
        message.affectedNodes = result.affectedNodes;
      }
      if (result.toolLimitReached) {
        message.text +=
          (message.text.length > 0 ? '\n\n' : '') +
          T(
            'I reached the tool-call limit for this turn, so I stopped before writing a summary. The steps shown above were executed - tell me to continue and I will finish and summarize what was done.',
            'ai_chat_tool_limit_reached',
          );
      }
    }

    thread.tokensUsed += result.usage.totalTokens;
    if (result.updatedSummary) {
      thread.summary = result.updatedSummary;
    }
    if (result.modelChanged) {
      this.refreshModelAfterChanges(result.affectedNodes);
    }
  }

  private pendingAssistantMessage(thread: ChatThread): ChatMessage | undefined {
    const lastMessage = thread.messages[thread.messages.length - 1];
    return lastMessage?.role === 'assistant' ? lastMessage : undefined;
  }

  private findThread(threadId: string): ChatThread | undefined {
    return this.threads.find(thread => thread.id === threadId);
  }

  private refreshModelAfterChanges(affectedNodes: AffectedNode[]) {
    const run = runInFlowWithHandler(this.rootStore.errorDialogController);
    const rootStore = this.rootStore;
    run({
      generator: function* () {
        yield* rootStore.modelTreeState.loadPackageNodes.bind(rootStore.modelTreeState)();
        const affectedIds = affectedNodes.map(node => node.origamId).filter(Boolean);
        if (affectedIds.length > 0) {
          yield* rootStore.editorTabViewState.reloadEditorsForOrigamIds(affectedIds)();
        }
      },
    });
  }

  @action
  private resetComposer() {
    this.draft = '';
    this.attachedImages = [];
  }

  private persistThreads() {
    const withoutImageData = this.threads.map(thread => ({
      ...thread,
      messages: thread.messages.map(message => ({
        id: message.id,
        role: message.role,
        text: message.text,
        calledFunctions: message.calledFunctions,
        affectedNodes: message.affectedNodes,
        totalTokens: message.totalTokens,
      })),
    }));
    try {
      localStorage.setItem(THREADS_STORAGE_KEY, JSON.stringify(withoutImageData));
    } catch {
      return;
    }
  }
}

function createThread(title: string): ChatThread {
  return {
    id: crypto.randomUUID(),
    title,
    messages: [],
    createdAt: Date.now(),
    tokensUsed: 0,
  };
}

function loadStoredThreads(): ChatThread[] {
  try {
    const raw = localStorage.getItem(THREADS_STORAGE_KEY);
    if (!raw) {
      return [];
    }
    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed)) {
      return [];
    }
    return parsed.map(thread => ({
      ...thread,
      tokensUsed: thread.tokensUsed ?? 0,
      messages: (thread.messages ?? []).map((message: ChatMessage) => ({
        ...message,
        id: message.id ?? crypto.randomUUID(),
      })),
    }));
  } catch {
    return [];
  }
}

function loadEnabledSections(): string[] {
  try {
    const raw = localStorage.getItem(ENABLED_SECTIONS_STORAGE_KEY);
    if (!raw) {
      return [...SAFE_DEFAULT_SECTIONS];
    }
    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed)) {
      return [...SAFE_DEFAULT_SECTIONS];
    }
    return parsed.filter(item => typeof item === 'string');
  } catch {
    return [...SAFE_DEFAULT_SECTIONS];
  }
}

function readFileAsDataUrl(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = () => reject(reader.error);
    reader.readAsDataURL(file);
  });
}

function tabIdToFocusItem(rootStore: RootStore, tabId: string, label: string): FocusItem {
  if (!GUID_PATTERN.test(tabId)) {
    return { label };
  }
  const node = rootStore.modelTreeState.findNodeById(tabId);
  return {
    label,
    origamId: tabId,
    itemTypeName: node?.itemTypeName ?? node?.nodeLevelType,
  };
}

function buildChatFocus(rootStore: RootStore): ChatFocus {
  const focus: ChatFocus = {};

  const activeState = rootStore.editorTabViewState.activeEditorState;
  if (activeState) {
    focus.activeEditor = tabIdToFocusItem(rootStore, activeState.tabId, activeState.label);
  }

  const openTabs: FocusItem[] = [];
  for (const container of rootStore.editorTabViewState.editorsContainers) {
    const state = container.state;
    if (!state.label) {
      continue;
    }
    if (activeState && state.tabId === activeState.tabId) {
      continue;
    }
    openTabs.push(tabIdToFocusItem(rootStore, state.tabId, state.label));
  }
  if (openTabs.length > 0) {
    focus.openTabs = openTabs;
  }

  const visibleNodes: FocusNode[] = [];

  function walkVisibleNodes(nodes: TreeNode[], path: string) {
    for (const node of nodes) {
      if (visibleNodes.length >= MAX_VISIBLE_NODES) {
        return;
      }
      visibleNodes.push({
        label: node.nodeText,
        itemTypeName: node.itemTypeName ?? node.nodeLevelType,
        origamId: node.origamId,
        path: path || 'root',
      });
      if (node.isExpanded && node.children.length > 0) {
        walkVisibleNodes(node.children, path ? `${path} / ${node.nodeText}` : node.nodeText);
      }
    }
  }

  walkVisibleNodes(rootStore.modelTreeState.modelNodes, '');
  if (visibleNodes.length > 0) {
    focus.visibleNodes = visibleNodes;
  }

  return focus;
}
