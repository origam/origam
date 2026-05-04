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

using System;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;

namespace Origam.Windows.Editor;

public partial class SqlEditor : UserControl
{
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    public SqlEditor()
    {
        InitializeComponent();
        editor.FontFamily = new System.Windows.Media.FontFamily(familyName: "Courier New");
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
        SearchPanel.Install(editor: editor);
        this.editor.TextArea.IndentationStrategy =
            new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
        editor.TextArea.Document.TextChanged += Document_TextChanged;
        using (
            var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream(name: "Origam.Windows.Editor.SQL.xshd")
        )
        {
            using (var reader = new System.Xml.XmlTextReader(input: stream))
            {
                editor.SyntaxHighlighting = HighlightingLoader.Load(
                    reader: reader,
                    resolver: HighlightingManager.Instance
                );
            }
        }
        var referenceBrush = new SimpleHighlightingBrush(
            color: System.Windows.Media.Color.FromRgb(r: 4, g: 139, b: 168)
        );
        var keywordBrush = new SimpleHighlightingBrush(
            color: System.Windows.Media.Color.FromRgb(r: 18, g: 53, b: 182)
        );
        var docBrush = new SimpleHighlightingBrush(
            color: System.Windows.Media.Color.FromRgb(r: 0, g: 154, b: 41)
        );
        var variableBrush = new SimpleHighlightingBrush(
            color: System.Windows.Media.Color.FromRgb(r: 255, g: 73, b: 61)
        );
        foreach (var color in editor.SyntaxHighlighting.NamedHighlightingColors)
        {
            switch (color.Name)
            {
                case "DataType":
                case "Keyword":
                {
                    color.Foreground = keywordBrush;
                    break;
                }

                case "Comment":
                {
                    color.Foreground = docBrush;
                    break;
                }

                case "Variable":
                {
                    color.Foreground = variableBrush;
                    break;
                }

                case "ObjectReference":
                case "ObjectReference1":
                case "ObjectReferenceInBrackets":
                case "ObjectReferenceInBrackets1":
                {
                    color.Foreground = referenceBrush;
                    break;
                }
            }
        }
    }

    public event ChangedEventHandler ContentChanged;

    // Invoke the Changed event; called whenever list changes
    protected virtual void OnContentChanged(EventArgs e)
    {
        if (ContentChanged != null)
        {
            ContentChanged(sender: this, e: e);
        }
    }

    private void Document_TextChanged(object sender, System.EventArgs e)
    {
        OnContentChanged(e: EventArgs.Empty);
    }

    public new string Text
    {
        get { return editor.Document.Text; }
        set
        {
            editor.Document.Text = value ?? "";
            editor.Document.UndoStack.ClearAll();
        }
    }
    public bool IsReadOnly
    {
        get { return editor.IsReadOnly; }
        set { editor.IsReadOnly = value; }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.H | Keys.Control))
        {
            FindReplaceForm frm = new FindReplaceForm();
            frm.Editor = editor;
            frm.Left = this.Width - frm.Width;
            frm.Top = this.PointToScreen(p: new System.Drawing.Point(x: 0, y: 0)).Y;
            frm.Show(owner: this.FindForm());
            return true;
        }
        return base.ProcessCmdKey(msg: ref msg, keyData: keyData);
    }
}
