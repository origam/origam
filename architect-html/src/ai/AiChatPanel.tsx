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

import { RootStoreContext, T } from '@/main';
import S from '@/ai/AiChatPanel.module.scss';
import { Markdown } from '@/ai/Markdown';
import { TreeNode } from '@components/modelTree/TreeNode';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { RootStore } from '@stores/RootStore';
import { useContext, useEffect, useRef, useState } from 'react';

type AffectedNode = {
  origamId: string;
  label?: string | null;
  itemTypeName?: string | null;
  action: string;
};

type ChatMessage = {
  role: 'user' | 'assistant';
  text: string;
  calledFunctions?: string[];
  affectedNodes?: AffectedNode[];
  images?: string[];
  totalTokens?: number;
};

type ChatThread = {
  id: string;
  title: string;
  messages: ChatMessage[];
  createdAt: number;
  tokensUsed: number;
  summary?: string;
};

type AttachedImage = {
  id: string;
  dataUrl: string;
};

type FocusItem = {
  label: string;
  itemTypeName?: string;
  origamId?: string;
};

type FocusNode = {
  label: string;
  itemTypeName?: string;
  origamId?: string;
  path?: string;
};

type ChatFocus = {
  activeEditor?: FocusItem;
  openTabs?: FocusItem[];
  visibleNodes?: FocusNode[];
};

type ApiSection = {
  name: string;
  functionCount: number;
  functions: string[];
  hasDestructive: boolean;
};

const THREADS_STORAGE_KEY = 'origam-ai-threads';
const ENABLED_SECTIONS_STORAGE_KEY = 'origam-ai-enabled-sections-v2';
const SAFE_DEFAULT_SECTIONS = ['Wizard', 'Search', 'Documentation', 'Tab'];
const MAX_VISIBLE_NODES = 40;
const GUID_PATTERN =
  /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

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
    return parsed.map(thread => ({ ...thread, tokensUsed: thread.tokensUsed ?? 0 }));
  } catch {
    return [];
  }
}

function storeThreads(threads: ChatThread[]) {
  const withoutImageData = threads.map(thread => ({
    ...thread,
    messages: thread.messages.map(message => ({
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

export function AiChatPanel() {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const defaultThreadTitle = T('New chat', 'ai_chat_thread_default');

  const [threads, setThreads] = useState<ChatThread[]>(() => {
    const stored = loadStoredThreads();
    return stored.length > 0 ? stored : [createThread(defaultThreadTitle)];
  });
  const [activeThreadId, setActiveThreadId] = useState<string>(() => threads[0].id);
  const [draft, setDraft] = useState('');
  const [attachedImages, setAttachedImages] = useState<AttachedImage[]>([]);
  const [isSending, setIsSending] = useState(false);
  const [errorText, setErrorText] = useState<string | null>(null);
  const [sections, setSections] = useState<ApiSection[]>([]);
  const [sectionsAvailable, setSectionsAvailable] = useState(true);
  const [enabledSections, setEnabledSections] = useState<string[]>(() => loadEnabledSections());
  const [isToolsOpen, setIsToolsOpen] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const abortControllerRef = useRef<AbortController | null>(null);
  const toolsRef = useRef<HTMLDivElement>(null);

  const activeThread = threads.find(thread => thread.id === activeThreadId) ?? threads[0];
  const messages = activeThread.messages;

  useEffect(() => {
    storeThreads(threads);
  }, [threads]);

  useEffect(() => {
    let cancelled = false;
    fetch('/aichat/sections')
      .then(response => response.json())
      .then(payload => {
        if (cancelled) {
          return;
        }
        setSectionsAvailable(payload.available !== false);
        setSections(Array.isArray(payload.sections) ? payload.sections : []);
      })
      .catch(() => {
        if (!cancelled) {
          setSectionsAvailable(false);
        }
      });
    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    try {
      localStorage.setItem(ENABLED_SECTIONS_STORAGE_KEY, JSON.stringify(enabledSections));
    } catch {
      return;
    }
  }, [enabledSections]);

  useEffect(() => {
    if (!isToolsOpen) {
      return;
    }
    function handleClickOutside(event: MouseEvent) {
      if (toolsRef.current && !toolsRef.current.contains(event.target as Node)) {
        setIsToolsOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [isToolsOpen]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isSending]);

  useEffect(() => {
    const textarea = textareaRef.current;
    if (!textarea) {
      return;
    }
    textarea.style.height = 'auto';
    textarea.style.height = Math.min(textarea.scrollHeight, 180) + 'px';
  }, [draft]);

  function createNewThread() {
    const thread = createThread(defaultThreadTitle);
    setThreads(previous => [thread, ...previous]);
    setActiveThreadId(thread.id);
    setDraft('');
    setAttachedImages([]);
    setErrorText(null);
  }

  function deleteActiveThread() {
    const remaining = threads.filter(thread => thread.id !== activeThreadId);
    const nextThreads = remaining.length > 0 ? remaining : [createThread(defaultThreadTitle)];
    setThreads(nextThreads);
    setActiveThreadId(nextThreads[0].id);
    setDraft('');
    setAttachedImages([]);
    setErrorText(null);
  }

  async function addImageFiles(files: FileList | File[] | null) {
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
    if (loaded.length > 0) {
      setAttachedImages(previous => [...previous, ...loaded]);
    }
  }

  function removeAttachedImage(imageId: string) {
    setAttachedImages(previous => previous.filter(image => image.id !== imageId));
  }

  function toggleSection(sectionName: string) {
    setEnabledSections(previous =>
      previous.includes(sectionName)
        ? previous.filter(name => name !== sectionName)
        : [...previous, sectionName],
    );
  }

  function refreshModelAfterChanges(affectedNodes: AffectedNode[]) {
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

  function openAffectedNode(node: AffectedNode) {
    if (node.action === 'deleted') {
      return;
    }
    run({
      generator: function* () {
        yield* rootStore.editorTabViewState.openEditorByOrigamId(node.origamId)();
        rootStore.modelTreeState.highlightNode(node.origamId);
      },
    });
  }

  async function sendMessage() {
    const trimmed = draft.trim();
    if ((trimmed.length === 0 && attachedImages.length === 0) || isSending) {
      return;
    }
    const threadId = activeThreadId;
    const imagesToSend = attachedImages.map(image => image.dataUrl);
    const messageText = trimmed || T('What is in this image?', 'ai_chat_default_image_prompt');
    const history = messages
      .filter(message => message.text.trim().length > 0)
      .map(message => ({ role: message.role, content: message.text }));
    const summary = activeThread.summary;

    setErrorText(null);
    setThreads(previous =>
      previous.map(thread =>
        thread.id === threadId
          ? {
              ...thread,
              title: thread.messages.length === 0 && trimmed ? trimmed.slice(0, 40) : thread.title,
              messages: [
                ...thread.messages,
                {
                  role: 'user',
                  text: messageText,
                  images: imagesToSend.length > 0 ? imagesToSend : undefined,
                },
              ],
            }
          : thread,
      ),
    );
    setDraft('');
    setAttachedImages([]);
    setIsSending(true);
    const abortController = new AbortController();
    abortControllerRef.current = abortController;
    try {
      const focus = buildChatFocus(rootStore);
      const httpResponse = await fetch('/aichat/message', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          message: messageText,
          focus,
          summary,
          history,
          images: imagesToSend,
          enabledSections,
        }),
        signal: abortController.signal,
      });
      if (!httpResponse.ok) {
        throw new Error('Request failed with status ' + httpResponse.status);
      }
      const payload = await httpResponse.json();
      const totalTokens = typeof payload.totalTokens === 'number' ? payload.totalTokens : 0;
      const updatedSummary =
        typeof payload.updatedSummary === 'string' && payload.updatedSummary.trim().length > 0
          ? payload.updatedSummary
          : undefined;
      const affectedNodes: AffectedNode[] = Array.isArray(payload.affectedNodes)
        ? payload.affectedNodes
        : [];
      setThreads(previous =>
        previous.map(thread =>
          thread.id === threadId
            ? {
                ...thread,
                tokensUsed: thread.tokensUsed + totalTokens,
                summary: updatedSummary ?? thread.summary,
                messages: [
                  ...thread.messages,
                  {
                    role: 'assistant',
                    text: payload.reply ?? '',
                    calledFunctions: payload.calledFunctions ?? [],
                    affectedNodes: affectedNodes.length > 0 ? affectedNodes : undefined,
                    totalTokens,
                  },
                ],
              }
            : thread,
        ),
      );
      if (payload.modelChanged) {
        refreshModelAfterChanges(affectedNodes);
      }
    } catch (error) {
      if ((error as Error).name !== 'AbortError') {
        setErrorText(
          T('Something went wrong. Is the AI service running on port 5210?', 'ai_chat_error'),
        );
      }
    } finally {
      abortControllerRef.current = null;
      setIsSending(false);
    }
  }

  function stopGeneration() {
    abortControllerRef.current?.abort();
  }

  function handleKeyDown(event: React.KeyboardEvent<HTMLTextAreaElement>) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      void sendMessage();
    }
  }

  function handlePaste(event: React.ClipboardEvent<HTMLTextAreaElement>) {
    const pastedFiles = Array.from(event.clipboardData.files);
    if (pastedFiles.some(file => file.type.startsWith('image/'))) {
      event.preventDefault();
      void addImageFiles(pastedFiles);
    }
  }

  return (
    <div className={S.panel}>
      <div className={S.header}>
        <div className={S.threadBar}>
          <select
            className={S.threadSelect}
            value={activeThread.id}
            onChange={event => setActiveThreadId(event.target.value)}
          >
            {threads.map(thread => (
              <option key={thread.id} value={thread.id}>
                {thread.title}
              </option>
            ))}
          </select>
          <button
            className={S.iconButton}
            title={T('New chat', 'ai_chat_new')}
            onClick={createNewThread}
          >
            +
          </button>
          <button
            className={S.iconButton}
            title={T('Delete chat', 'ai_chat_delete')}
            onClick={deleteActiveThread}
          >
            ✕
          </button>
          <div className={S.toolsWrapper} ref={toolsRef}>
            <button
              className={S.iconButton}
              title={T('Tools', 'ai_chat_tools')}
              onClick={() => setIsToolsOpen(open => !open)}
            >
              🧩
              {enabledSections.length > 0 && (
                <span className={S.toolsCount}>{enabledSections.length}</span>
              )}
            </button>
            {isToolsOpen && (
              <div className={S.toolsPopover}>
                <div className={S.toolsPopoverHeader}>
                  <div className={S.toolsTitle}>{T('API sections', 'ai_chat_tools_title')}</div>
                  <div className={S.toolsSubtitle}>
                    {T(
                      'Turn Swagger sections on or off. The assistant can only call tools from sections that are on.',
                      'ai_chat_tools_subtitle',
                    )}
                  </div>
                </div>
                {!sectionsAvailable && (
                  <div className={S.toolsUnavailable}>
                    {T(
                      'Sections unavailable. Is the Architect server running with Swagger?',
                      'ai_chat_tools_unavailable',
                    )}
                  </div>
                )}
                <div className={S.toolsList}>
                  {sections.map(section => {
                    const isEnabled = enabledSections.includes(section.name);
                    return (
                      <div key={section.name} className={S.toolsItem}>
                        <div className={S.toolsItemInfo}>
                          <div className={S.toolsItemName}>
                            {section.name}
                            {section.hasDestructive && (
                              <span
                                className={S.toolsCaution}
                                title={T(
                                  'Contains destructive operations (delete, deploy). Off by default.',
                                  'ai_chat_tools_caution',
                                )}
                              >
                                {T('caution', 'ai_chat_tools_caution_badge')}
                              </span>
                            )}
                          </div>
                          <div className={S.toolsItemMeta}>
                            {T(
                              '{0} functions',
                              'ai_chat_tools_functions',
                              section.functionCount.toString(),
                            )}
                          </div>
                        </div>
                        <button
                          type="button"
                          role="switch"
                          aria-checked={isEnabled}
                          className={isEnabled ? `${S.switch} ${S.switchOn}` : S.switch}
                          onClick={() => toggleSection(section.name)}
                        >
                          <span className={S.switchKnob} />
                        </button>
                      </div>
                    );
                  })}
                </div>
              </div>
            )}
          </div>
        </div>
        {activeThread.tokensUsed > 0 && (
          <div className={S.tokenBar}>
            {T('Tokens used: {0}', 'ai_chat_tokens', activeThread.tokensUsed.toLocaleString())}
          </div>
        )}
      </div>

      <div className={S.messages}>
        {messages.length === 0 && !errorText && (
          <div className={S.empty}>
            {T('No messages yet. Try: "What time is it?"', 'ai_chat_empty')}
          </div>
        )}

        {messages.map((message, messageIndex) => (
          <div
            key={messageIndex}
            className={
              message.role === 'user' ? `${S.row} ${S.rowUser}` : `${S.row} ${S.rowAssistant}`
            }
          >
            <div
              className={
                message.role === 'user'
                  ? `${S.bubble} ${S.bubbleUser}`
                  : `${S.bubble} ${S.bubbleAssistant}`
              }
            >
              <div className={S.role}>
                {message.role === 'user'
                  ? T('You', 'ai_chat_role_user')
                  : T('Assistant', 'ai_chat_role_assistant')}
              </div>
              {message.images && message.images.length > 0 && (
                <div className={S.messageImages}>
                  {message.images.map((imageSource, imageIndex) => (
                    <img
                      key={imageIndex}
                      className={S.messageImage}
                      src={imageSource}
                      alt={T('Attached image', 'ai_chat_attached_image')}
                    />
                  ))}
                </div>
              )}
              {message.role === 'assistant'
                ? message.text.length > 0 && <Markdown text={message.text} />
                : message.text}
              {message.affectedNodes && message.affectedNodes.length > 0 && (
                <div className={S.affectedNodes}>
                  {T('Created / changed:', 'ai_chat_affected_nodes')}
                  <div>
                    {message.affectedNodes.map(node => {
                      const isDeleted = node.action === 'deleted';
                      const openEditorLabel = rootStore.editorTabViewState.editorsContainers.find(
                        editor => editor.state.origamId === node.origamId,
                      )?.state.label;
                      const chipLabel =
                        node.label ||
                        rootStore.modelTreeState.findNodeById(node.origamId)?.nodeText ||
                        openEditorLabel ||
                        node.itemTypeName ||
                        T('item', 'ai_chat_affected_fallback');
                      return (
                        <button
                          key={node.origamId}
                          type="button"
                          className={
                            isDeleted
                              ? `${S.affectedChip} ${S.affectedChipDeleted}`
                              : S.affectedChip
                          }
                          title={node.itemTypeName ?? undefined}
                          disabled={isDeleted}
                          onClick={() => openAffectedNode(node)}
                        >
                          {chipLabel}
                        </button>
                      );
                    })}
                  </div>
                </div>
              )}
              {message.calledFunctions && message.calledFunctions.length > 0 && (
                <div className={S.calledFunctions}>
                  {T('Called functions:', 'ai_chat_called_functions')}
                  <div>
                    {message.calledFunctions.map(functionName => (
                      <span key={functionName} className={S.pill}>
                        {functionName}
                      </span>
                    ))}
                  </div>
                </div>
              )}
              {message.role === 'assistant' && !!message.totalTokens && (
                <div className={S.tokenNote}>
                  {T('{0} tokens', 'ai_chat_msg_tokens', message.totalTokens.toLocaleString())}
                </div>
              )}
            </div>
          </div>
        ))}

        {isSending && (
          <div className={`${S.row} ${S.rowAssistant}`}>
            <div className={`${S.bubble} ${S.bubbleAssistant}`}>
              {T('Thinking…', 'ai_chat_thinking')}
            </div>
          </div>
        )}

        {errorText && <div className={S.error}>{errorText}</div>}

        <div ref={messagesEndRef} />
      </div>

      <div className={S.composer}>
        {attachedImages.length > 0 && (
          <div className={S.thumbnails}>
            {attachedImages.map(image => (
              <div key={image.id} className={S.thumbnail}>
                <img
                  className={S.thumbnailImage}
                  src={image.dataUrl}
                  alt={T('Attached image', 'ai_chat_attached_image')}
                />
                <button
                  className={S.thumbnailRemove}
                  title={T('Remove image', 'ai_chat_remove_image')}
                  onClick={() => removeAttachedImage(image.id)}
                >
                  ✕
                </button>
              </div>
            ))}
          </div>
        )}
        <div className={S.composerRow}>
          <button
            className={S.attachButton}
            title={T('Attach image', 'ai_chat_attach')}
            onClick={() => fileInputRef.current?.click()}
            disabled={isSending}
          >
            📎
          </button>
          <textarea
            ref={textareaRef}
            className={S.textarea}
            value={draft}
            onChange={event => setDraft(event.target.value)}
            onKeyDown={handleKeyDown}
            onPaste={handlePaste}
            placeholder={T(
              'Message…  (Enter to send, Shift+Enter for newline)',
              'ai_chat_placeholder',
            )}
            disabled={isSending}
          />
          {isSending ? (
            <button className={S.sendButton} onClick={stopGeneration}>
              {T('Stop', 'ai_chat_stop')}
            </button>
          ) : (
            <button
              className={S.sendButton}
              onClick={() => void sendMessage()}
              disabled={draft.trim().length === 0 && attachedImages.length === 0}
            >
              {T('Send', 'ai_chat_send')}
            </button>
          )}
        </div>
        <input
          ref={fileInputRef}
          className={S.hiddenInput}
          type="file"
          accept="image/*"
          multiple
          onChange={event => {
            void addImageFiles(event.target.files);
            event.target.value = '';
          }}
        />
      </div>
    </div>
  );
}
