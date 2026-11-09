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

import * as monaco from 'monaco-editor/editor/editor.api.js';

const XslNamespacePattern =
  /xmlns:([\w.-]+)\s*=\s*["']http:\/\/www\.w3\.org\/1999\/XSL\/Transform["']/;
const OpenTagBeforeCursorPattern = /<([A-Za-z_][\w:.-]*)(?:\s[^<>]*)?>$/;
const TagNameBeforeCursorPattern = /<([\w:.-]*)$/;
const LookBehindCharacters = 4000;

interface XslElement {
  name: string;
  attributes: string[];
  isEmpty: boolean;
}

const xslElements: XslElement[] = [
  { name: 'apply-imports', attributes: [], isEmpty: true },
  { name: 'apply-templates', attributes: ['select'], isEmpty: true },
  { name: 'attribute', attributes: ['name'], isEmpty: false },
  { name: 'attribute-set', attributes: ['name'], isEmpty: false },
  { name: 'call-template', attributes: ['name'], isEmpty: false },
  { name: 'choose', attributes: [], isEmpty: false },
  { name: 'comment', attributes: [], isEmpty: false },
  { name: 'copy', attributes: [], isEmpty: false },
  { name: 'copy-of', attributes: ['select'], isEmpty: true },
  { name: 'decimal-format', attributes: ['name'], isEmpty: true },
  { name: 'element', attributes: ['name'], isEmpty: false },
  { name: 'fallback', attributes: [], isEmpty: false },
  { name: 'for-each', attributes: ['select'], isEmpty: false },
  { name: 'if', attributes: ['test'], isEmpty: false },
  { name: 'import', attributes: ['href'], isEmpty: true },
  { name: 'include', attributes: ['href'], isEmpty: true },
  { name: 'key', attributes: ['name', 'match', 'use'], isEmpty: true },
  { name: 'message', attributes: [], isEmpty: false },
  { name: 'namespace-alias', attributes: ['stylesheet-prefix', 'result-prefix'], isEmpty: true },
  { name: 'number', attributes: ['value'], isEmpty: true },
  { name: 'otherwise', attributes: [], isEmpty: false },
  { name: 'output', attributes: ['method'], isEmpty: true },
  { name: 'param', attributes: ['name'], isEmpty: false },
  { name: 'preserve-space', attributes: ['elements'], isEmpty: true },
  { name: 'processing-instruction', attributes: ['name'], isEmpty: false },
  { name: 'sort', attributes: ['select'], isEmpty: true },
  { name: 'strip-space', attributes: ['elements'], isEmpty: true },
  { name: 'stylesheet', attributes: ['version'], isEmpty: false },
  { name: 'template', attributes: ['match'], isEmpty: false },
  { name: 'text', attributes: [], isEmpty: false },
  { name: 'transform', attributes: ['version'], isEmpty: false },
  { name: 'value-of', attributes: ['select'], isEmpty: true },
  { name: 'variable', attributes: ['name', 'select'], isEmpty: true },
  { name: 'when', attributes: ['test'], isEmpty: false },
  { name: 'with-param', attributes: ['name', 'select'], isEmpty: true },
];

function toSnippet(element: XslElement, prefix: string) {
  const qualifiedName = `${prefix}:${element.name}`;
  const attributes = element.attributes
    .map((attribute, index) => ` ${attribute}="$${index + 1}"`)
    .join('');
  const ending = element.isEmpty ? '/>$0' : `>$0</${qualifiedName}>`;
  return qualifiedName + attributes + ending;
}

function getTextBefore(model: monaco.editor.ITextModel, position: monaco.IPosition) {
  const endOffset = model.getOffsetAt(position);
  const start = model.getPositionAt(Math.max(0, endOffset - LookBehindCharacters));
  return model.getValueInRange({
    startLineNumber: start.lineNumber,
    startColumn: start.column,
    endLineNumber: position.lineNumber,
    endColumn: position.column,
  });
}

monaco.languages.registerCompletionItemProvider('xml', {
  triggerCharacters: ['<', ':'],
  provideCompletionItems(model, position) {
    const namespaceMatch = model.getValue().match(XslNamespacePattern);
    if (!namespaceMatch) {
      return { suggestions: [] };
    }
    const lineBeforeCursor = model
      .getLineContent(position.lineNumber)
      .substring(0, position.column - 1);
    const tagNameMatch = lineBeforeCursor.match(TagNameBeforeCursorPattern);
    if (!tagNameMatch) {
      return { suggestions: [] };
    }
    const nextCharacter = model.getLineContent(position.lineNumber).charAt(position.column - 1);
    const range = new monaco.Range(
      position.lineNumber,
      position.column - tagNameMatch[1].length,
      position.lineNumber,
      nextCharacter === '>' ? position.column + 1 : position.column,
    );
    const prefix = namespaceMatch[1];
    return {
      suggestions: xslElements.map(element => ({
        label: `${prefix}:${element.name}`,
        kind: monaco.languages.CompletionItemKind.Keyword,
        insertText: toSnippet(element, prefix),
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
        range,
      })),
    };
  },
});

export function enableXmlTagAutoClose(editor: monaco.editor.ICodeEditor) {
  return editor.onDidChangeModelContent(event => {
    if (event.isUndoing || event.isRedoing || event.changes.length !== 1) {
      return;
    }
    const change = event.changes[0];
    const model = editor.getModel();
    if (change.text !== '>' || !model || model.getLanguageId() !== 'xml') {
      return;
    }
    const position = {
      lineNumber: change.range.startLineNumber,
      column: change.range.startColumn + 1,
    };
    const textBefore = getTextBefore(model, position);
    const openTagMatch = textBefore.match(OpenTagBeforeCursorPattern);
    if (!openTagMatch || textBefore.endsWith('/>')) {
      return;
    }
    const closingTag = `</${openTagMatch[1]}>`;
    editor.executeEdits('xml-tag-auto-close', [
      {
        range: new monaco.Range(
          position.lineNumber,
          position.column,
          position.lineNumber,
          position.column,
        ),
        text: closingTag,
      },
    ]);
    editor.setPosition(position);
  });
}
