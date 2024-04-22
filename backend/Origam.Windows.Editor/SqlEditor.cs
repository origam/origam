#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
#endregion

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace Origam.Windows.Editor;

public partial class SqlEditor : UserControl
{
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    public SqlEditor()
    {
            InitializeComponent();
            editor.FontFamily = new System.Windows.Media.FontFamily("Courier New");
            editor.FontSize = 12;
            TextEditorOptions options = new TextEditorOptions();
            options.ConvertTabsToSpaces = false;
            options.HighlightCurrentLine = true;
            options.ShowEndOfLine = true;
            options.ShowSpaces = true;
            options.ShowTabs = true;
            options.ShowColumnRuler = true;
            options.ColumnRulerPosition = 80;
            editor.Options = options;
            editor.ShowLineNumbers = true;
            SearchPanel.Install(editor);            
            this.editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
            editor.TextArea.Document.TextChanged += Document_TextChanged;
            using (var stream = Assembly.GetExecutingAssembly().
                GetManifestResourceStream("Origam.Windows.Editor.SQL.xshd"))
            {
                using (var reader = new System.Xml.XmlTextReader(stream))
                {
                    editor.SyntaxHighlighting =
                        HighlightingLoader.Load(reader,
                        HighlightingManager.Instance);
                }
            }
            var referenceBrush = new SimpleHighlightingBrush(
                System.Windows.Media.Color.FromRgb(4, 139, 168));
            var keywordBrush = new SimpleHighlightingBrush(
                System.Windows.Media.Color.FromRgb(18, 53, 182));
            var docBrush = new SimpleHighlightingBrush(
                System.Windows.Media.Color.FromRgb(0, 154, 41));
            var variableBrush = new SimpleHighlightingBrush(
                System.Windows.Media.Color.FromRgb(255, 73, 61));
            foreach (var color in editor.SyntaxHighlighting.NamedHighlightingColors)
            {
                switch (color.Name)
                {
                    case "DataType":
                    case "Keyword":
                        color.Foreground = keywordBrush;
                        break;
                    case "Comment":
                        color.Foreground = docBrush;
                        break;
                    case "Variable":
                        color.Foreground = variableBrush;
                        break;
                    case "ObjectReference":
                    case "ObjectReference1":
                    case "ObjectReferenceInBrackets":
                    case "ObjectReferenceInBrackets1":
                        color.Foreground = referenceBrush;
                        break;
                }
            }
        }

    public event ChangedEventHandler ContentChanged;

    // Invoke the Changed event; called whenever list changes
    protected virtual void OnContentChanged(EventArgs e)
    {
            if (ContentChanged != null)
            {
                ContentChanged(this, e);
            }
        }

    private void Document_TextChanged(object sender, System.EventArgs e)
    {
            OnContentChanged(EventArgs.Empty);
        }

    public new string Text
    {
        get
        {
                return editor.Document.Text;
            }
        set
        {
                editor.Document.Text = value ?? "";
                editor.Document.UndoStack.ClearAll();
            }
    }

    public bool IsReadOnly
    {
        get
        {
                return editor.IsReadOnly;
            }
        set
        {
                editor.IsReadOnly = value;
            }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
            if (keyData == (Keys.H | Keys.Control))
            {
                FindReplaceForm frm = new FindReplaceForm();
                frm.Editor = editor;
                frm.Left = this.Width - frm.Width;
                frm.Top = this.PointToScreen(new System.Drawing.Point(0, 0)).Y;
                frm.Show(this.FindForm());
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
}