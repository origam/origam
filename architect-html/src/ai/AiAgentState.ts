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
  AttachedImage,
  ChatFocus,
  ChatImage,
  ChatMessage,
  ChatThread,
  ChatToolCall,
  FocusItem,
  FocusNode,
} from '@/ai/AiAgentTypes';
import {
  createArchitectAgent,
  parseRunResult,
  RUN_RESULT_EVENT_NAME,
} from '@/ai/agui/ArchitectAgentClient';
import { getAgentConnection } from '@/ai/AiPromptApi';
import {
  blobToDataUrl,
  deleteChatThread,
  fetchChatImageAsDataUrl,
  getChatThreads,
  saveChatImage,
  saveChatThread,
} from '@/ai/ChatHistoryApi';
import { T } from '@/main';
import { AgentSubscriber, HttpAgent } from '@ag-ui/client';
import { HttpError } from '@api/httpClient';
import { ITabState } from '@components/editorTabView/ITabState';
import { TreeNode } from '@components/modelTree/TreeNode';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { RootStore } from '@stores/RootStore';
import { action, observable, toJS } from 'mobx';

const MAX_VISIBLE_NODES = 40;
const HISTORY_RETRY_DELAYS = [1000, 2000, 4000, 8000];
const MAX_IMAGE_BYTES = 10 * 1024 * 1024;
const MAX_TOOL_RESULT_CHARS = 8000;
const SUPPORTED_IMAGE_MIME_TYPES = ['image/png', 'image/jpeg', 'image/gif', 'image/webp'];

export class AiAgentState {
  @observable accessor threads: ChatThread[] = [];
  @observable accessor activeThreadId: string = '';
  @observable accessor draft: string = '';
  @observable accessor attachedImages: AttachedImage[] = [];
  @observable accessor isRunning: boolean = false;
  @observable accessor errorText: string | null = null;
  @observable accessor isApiKeyMissing: boolean = false;

  private runningAgent: HttpAgent | null = null;
  private hasLoadedHistory: boolean = false;
  private isAborting: boolean = false;
  private streamedMessageIds = new Map<string, string>();
  private pendingThreadSave: Promise<void> = Promise.resolve();

  constructor(private rootStore: RootStore) {
    this.threads = [createThread(this.defaultThreadTitle)];
    this.activeThreadId = this.threads[0].id;
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

  async loadHistory() {
    for (let attempt = 0; ; attempt++) {
      try {
        this.applyLoadedThreads(await getChatThreads());
        return;
      } catch {
        if (attempt >= HISTORY_RETRY_DELAYS.length) {
          this.setHistoryError();
          return;
        }
        await delay(HISTORY_RETRY_DELAYS[attempt]);
      }
    }
  }

  async loadConnection() {
    try {
      const connection = await getAgentConnection();
      this.setApiKeyMissing(!connection.hasApiKey);
    } catch {
      return;
    }
  }

  @action
  private setApiKeyMissing(isApiKeyMissing: boolean) {
    this.isApiKeyMissing = isApiKeyMissing;
  }

  @action
  private applyLoadedThreads(loadedThreads: ChatThread[]) {
    const keptThreads = this.threads.filter(
      thread =>
        thread.messages.length > 0 || (this.hasLoadedHistory && thread.id === this.activeThreadId),
    );
    const keptThreadIds = new Set(keptThreads.map(thread => thread.id));
    const merged = [
      ...keptThreads,
      ...loadedThreads.filter(thread => !keptThreadIds.has(thread.id)),
    ];
    if (merged.length === 0) {
      return;
    }
    this.threads = merged;
    if (!merged.some(thread => thread.id === this.activeThreadId)) {
      this.activeThreadId = merged[0].id;
    }
    this.hasLoadedHistory = true;
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
  }

  @action
  deleteActiveThread() {
    const deletedThreadId = this.activeThreadId;
    const remaining = this.threads.filter(thread => thread.id !== deletedThreadId);
    this.threads = remaining.length > 0 ? remaining : [createThread(this.defaultThreadTitle)];
    this.activeThreadId = this.threads[0].id;
    this.resetComposer();
    void deleteChatThread(deletedThreadId).catch((error: HttpError) => {
      if (error.status !== 404) {
        this.setHistoryError();
      }
    });
  }

  @action
  setDraft(draft: string) {
    this.draft = draft;
  }

  async addImageFiles(files: FileList | File[] | null) {
    if (!files) {
      return;
    }
    const candidates = Array.from(files).filter(file => file.type.startsWith('image/'));
    const supported = candidates.filter(
      file => SUPPORTED_IMAGE_MIME_TYPES.includes(file.type) && file.size <= MAX_IMAGE_BYTES,
    );
    if (supported.length < candidates.length) {
      this.setImageRejectedError();
    }
    const loaded = await Promise.all(
      supported.map(async file => ({
        id: crypto.randomUUID(),
        mimeType: file.type,
        dataUrl: await blobToDataUrl(file),
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
    const images: ChatImage[] = this.attachedImages.map(image => ({
      id: image.id,
      mimeType: image.mimeType,
      dataUrl: image.dataUrl,
    }));

    this.startRun(thread, messageText, images);
    void this.uploadImages(thread.id, images);
    await this.hydrateImages(thread);
    await this.rootStore.aiToolSectionsState.load();

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
            enabledSections: this.rootStore.aiToolSectionsState.enabledSections,
            summary: thread.summary,
          },
        },
        this.buildSubscriber(thread.id),
      );
    } catch (error) {
      this.reportRunError(error);
    } finally {
      this.finishRun(thread);
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
        this.addCalledFunction(threadId, event.toolCallName, event.toolCallId);
      },
      onToolCallArgsEvent: ({ event }) => {
        this.appendToolCallArguments(threadId, event.toolCallId, event.delta);
      },
      onToolCallResultEvent: ({ event }) => {
        this.setToolCallResult(threadId, event.toolCallId, event.content);
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
  private startRun(thread: ChatThread, messageText: string, images: ChatImage[]) {
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
    this.persistThread(thread);
  }

  @action
  private finishRun(thread: ChatThread) {
    this.isRunning = false;
    this.isAborting = false;
    this.runningAgent = null;
    this.persistThread(thread);
  }

  @action
  private reportRunError(error: unknown) {
    if (this.isAborting || (error as Error)?.name === 'AbortError') {
      return;
    }
    this.errorText = T(
      'Something went wrong. Is the Architect server running and the AI API key configured?',
      'ai_chat_error',
    );
  }

  @action
  private setErrorText(message: string | undefined) {
    this.errorText =
      message && message.trim().length > 0
        ? message
        : T(
            'Something went wrong. Is the Architect server running and the AI API key configured?',
            'ai_chat_error',
          );
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
  private addCalledFunction(threadId: string, functionName: string, toolCallId: string) {
    const thread = this.findThread(threadId);
    if (!thread) {
      return;
    }
    if (!this.pendingAssistantMessage(thread)) {
      thread.messages.push({ id: crypto.randomUUID(), role: 'assistant', text: '' });
    }
    const message = this.pendingAssistantMessage(thread);
    if (message) {
      message.calledFunctions = [...(message.calledFunctions ?? []), functionName];
      message.toolCalls = [
        ...(message.toolCalls ?? []),
        { id: toolCallId, name: functionName, arguments: '' },
      ];
    }
  }

  @action
  private appendToolCallArguments(threadId: string, toolCallId: string, delta: string) {
    const toolCall = this.findToolCall(threadId, toolCallId);
    if (toolCall) {
      toolCall.arguments += delta;
    }
  }

  @action
  private setToolCallResult(threadId: string, toolCallId: string, content: string) {
    const toolCall = this.findToolCall(threadId, toolCallId);
    if (toolCall) {
      toolCall.result =
        content.length <= MAX_TOOL_RESULT_CHARS
          ? content
          : `${content.slice(0, MAX_TOOL_RESULT_CHARS)}... [truncated]`;
    }
  }

  private findToolCall(threadId: string, toolCallId: string): ChatToolCall | undefined {
    const thread = this.findThread(threadId);
    if (!thread) {
      return undefined;
    }
    for (let index = thread.messages.length - 1; index >= 0; index--) {
      const toolCall = thread.messages[index].toolCalls?.find(
        candidate => candidate.id === toolCallId,
      );
      if (toolCall) {
        return toolCall;
      }
    }
    return undefined;
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

  @action
  private setHistoryError() {
    this.errorText = T(
      'Chat history could not be read or saved on the server.',
      'ai_chat_history_error',
    );
  }

  @action
  private setImageRejectedError() {
    this.errorText = T(
      'Only PNG, JPEG, GIF and WebP images up to 10 MB can be attached.',
      'ai_chat_image_rejected',
    );
  }

  private persistThread(thread: ChatThread) {
    this.pendingThreadSave = saveChatThread(withoutImageData(thread)).catch(() =>
      this.setHistoryError(),
    );
  }

  private async uploadImages(threadId: string, images: ChatImage[]) {
    if (images.length === 0) {
      return;
    }
    await this.pendingThreadSave;
    for (const image of images) {
      if (!image.dataUrl) {
        continue;
      }
      try {
        await saveChatImage(threadId, image.id, image.mimeType, image.dataUrl);
      } catch {
        this.setHistoryError();
      }
    }
  }

  private async hydrateImages(thread: ChatThread) {
    const missing = thread.messages.flatMap(message =>
      (message.images ?? []).filter(image => !image.dataUrl),
    );
    await Promise.all(
      missing.map(async image => {
        try {
          this.applyImageData(image, await fetchChatImageAsDataUrl(thread.id, image.id));
        } catch {
          return;
        }
      }),
    );
  }

  @action
  private applyImageData(image: ChatImage, dataUrl: string) {
    image.dataUrl = dataUrl;
  }
}

function delay(milliseconds: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, milliseconds));
}

function createThread(title: string): ChatThread {
  return {
    id: crypto.randomUUID(),
    title,
    messages: [],
    createdAt: new Date().toISOString(),
    tokensUsed: 0,
  };
}

function withoutImageData(thread: ChatThread): ChatThread {
  const plainThread = toJS(thread);
  return {
    ...plainThread,
    messages: plainThread.messages.map(message => ({
      ...message,
      images: message.images?.map(image => ({ id: image.id, mimeType: image.mimeType })),
    })),
  };
}

function tabStateToFocusItem(rootStore: RootStore, state: ITabState): FocusItem {
  if (!state.origamId) {
    return { label: state.label };
  }
  const node = rootStore.modelTreeState.findNodeById(state.origamId);
  return {
    label: state.label,
    origamId: state.origamId,
    itemTypeName: node?.itemTypeName ?? node?.nodeLevelType,
  };
}

function buildChatFocus(rootStore: RootStore): ChatFocus {
  const focus: ChatFocus = {};

  const activeState = rootStore.editorTabViewState.activeEditorState;
  if (activeState) {
    focus.activeEditor = tabStateToFocusItem(rootStore, activeState);
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
    openTabs.push(tabStateToFocusItem(rootStore, state));
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
