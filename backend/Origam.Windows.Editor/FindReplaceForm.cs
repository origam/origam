using System;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace Origam.Windows.Editor;

public partial class FindReplaceForm : Form
{
    public TextEditor Editor { get; set; }

    public FindReplaceForm()
    {
        InitializeComponent();
        this.Shown += FindReplaceForm_Shown;
    }

    private void FindReplaceForm_Shown(object sender, EventArgs e)
    {
        if (!Editor.TextArea.Selection.IsMultiline)
        {
            txtFind.Text = Editor.TextArea.Selection.GetText();
            txtFind.SelectAll();
        }
    }

    private void btnFindNext_Click(object sender, EventArgs e)
    {
        if (!FindNext(textToFind: txtFind.Text))
        {
            SystemSounds.Beep.Play();
        }
    }

    private void btnReplace_Click(object sender, EventArgs e)
    {
        Regex regex = GetRegEx(textToFind: txtFind.Text);
        string input = Editor.Text.Substring(
            startIndex: Editor.SelectionStart,
            length: Editor.SelectionLength
        );
        Match match = regex.Match(input: input);
        bool replaced = false;
        if (match.Success && match.Index == 0 && match.Length == input.Length)
        {
            Editor.Document.Replace(
                offset: Editor.SelectionStart,
                length: Editor.SelectionLength,
                text: txtReplace.Text
            );
            replaced = true;
        }
        if (!FindNext(textToFind: txtFind.Text) && !replaced)
        {
            SystemSounds.Beep.Play();
        }
    }

    private void btnReplaceAll_Click(object sender, EventArgs e)
    {
        if (
            MessageBox.Show(
                text: "Are you sure you want to Replace All occurences of \""
                    + txtFind.Text
                    + "\" with \""
                    + txtReplace.Text
                    + "\"?",
                caption: "Replace All",
                buttons: MessageBoxButtons.OKCancel,
                icon: MessageBoxIcon.Question
            ) == DialogResult.OK
        )
        {
            Regex regex = GetRegEx(textToFind: txtFind.Text, leftToRight: true);
            int offset = 0;
            Editor.BeginChange();
            foreach (Match match in regex.Matches(input: Editor.Text))
            {
                Editor.Document.Replace(
                    offset: offset + match.Index,
                    length: match.Length,
                    text: txtReplace.Text
                );
                offset += txtReplace.Text.Length - match.Length;
            }
            Editor.EndChange();
        }
    }

    private bool FindNext(string textToFind)
    {
        Regex regex = GetRegEx(textToFind: textToFind);
        int start = regex.Options.HasFlag(flag: RegexOptions.RightToLeft)
            ? Editor.SelectionStart
            : Editor.SelectionStart + Editor.SelectionLength;
        Match match = regex.Match(input: Editor.Text, startat: start);
        if (!match.Success) // start again from beginning or end
        {
            if (regex.Options.HasFlag(flag: RegexOptions.RightToLeft))
            {
                match = regex.Match(input: Editor.Text, startat: Editor.Text.Length);
            }
            else
            {
                match = regex.Match(input: Editor.Text, startat: 0);
            }
        }
        if (match.Success)
        {
            Editor.Select(start: match.Index, length: match.Length);
            TextLocation loc = Editor.Document.GetLocation(offset: match.Index);
            Editor.ScrollTo(line: loc.Line, column: loc.Column);
        }
        return match.Success;
    }

    private Regex GetRegEx(string textToFind, bool leftToRight = false)
    {
        RegexOptions options = RegexOptions.None;
        if (cbSearchUp.Checked && !leftToRight)
        {
            options |= RegexOptions.RightToLeft;
        }

        if (!cbCaseSensitive.Checked)
        {
            options |= RegexOptions.IgnoreCase;
        }

        if (cbRegex.Checked == true)
        {
            return new Regex(pattern: textToFind, options: options);
        }
        string pattern = Regex.Escape(str: textToFind);
        if (cbWildcards.Checked)
        {
            pattern = pattern
                .Replace(oldValue: "\\*", newValue: ".*")
                .Replace(oldValue: "\\?", newValue: ".");
        }

        if (cbWholeWord.Checked)
        {
            pattern = "\\b" + pattern + "\\b";
        }

        return new Regex(pattern: pattern, options: options);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            this.Close();
            return true;
        }
        return base.ProcessCmdKey(msg: ref msg, keyData: keyData);
    }
}
